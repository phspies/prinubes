using System;
using System.Collections;
using System.Net;

namespace PlatformWorker
{
    public class PlatformWorker
    {

        public Log log;
        private String _cname;
        private Hashtable optsEntered = new Hashtable();
        private Hashtable userOpts = new Hashtable();
        private Hashtable builtInOpts = new Hashtable();
        private String logfilepath = "";
        public SvcConnection _connection;
        public ServiceUtil _svcUtil;
        private static Log gLog;

        public static PlatformWorker initialize(String name, OptionSpec[] userOptions, String[] args)
        {
            PlatformWorker cb = new PlatformWorker(name);
            return cb;
        }

        public static PlatformWorker initialize(String name, String[] args)
        {
            PlatformWorker cb = initialize(name, null, args);
            return cb;
        }
        public static void ALog(Log glog)
        {
            gLog = glog;
        }

        public PlatformWorker(String name)
        {
            setup();
            init(name);
        }

        public void setup()
        {
            _svcUtil = new ServiceUtil();
            _connection = new SvcConnection("ServiceInstance");
        }

        public void init(String name)
        {
            _cname = name;
        }

        public void initConnection()
        {
            _svcUtil.Init(this);
        }
        private void getCmdArguments(string[] args)
        {
            int len = args.Length;
            int i = 0;

            if (len == 0)
            {
                displayUsage();
            }
            while (i < args.Length)
            {
                String val = "";
                String opt = args[i];
                if (opt.StartsWith("--") && optsEntered.ContainsKey(opt.Substring(2)))
                {
                    Console.WriteLine("key '" + opt.Substring(2) + "' already exists ");
                    displayUsage();
                }
                if (args[i].StartsWith("--"))
                {
                    if (args.Length > i + 1)
                    {
                        if (!args[i + 1].StartsWith("--"))
                        {
                            val = args[i + 1];
                            optsEntered[opt.Substring(2)] = val;
                        }
                        else
                        {
                            optsEntered[opt.Substring(2)] = null;
                        }
                    }
                    else
                    {
                        optsEntered[opt.Substring(2)] = null;
                    }
                }
                i++;
            }
        }

        private Boolean checkDatatypes(Hashtable Opts, String keyValue, String keyOptions)
        {
            Boolean valid = false;
            valid = Opts.ContainsKey(keyValue);
            if (valid)
            {
                OptionSpec oSpec = (OptionSpec)Opts[keyValue];
                String dataType = oSpec.getOptionType();
                Boolean result = validateDataType(dataType, keyOptions);
                return result;
            }
            else
            {
                return false;
            }
        }

        private Boolean validateDataType(String dataType, String keyValue)
        {
            try
            {
                if (dataType.Equals("Boolean"))
                {
                    if (keyValue.Equals("true") || keyValue.Equals("false"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (dataType.Equals("Integer"))
                {
                    int val = Int32.Parse(keyValue);
                    return true;
                }
                else if (dataType.Equals("Float"))
                {
                    float.Parse(keyValue);
                    return true;
                }
                else if (dataType.Equals("Long"))
                {
                    long.Parse(keyValue);
                    return true;
                }
                else
                {
                    // DO NOTHING
                }
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        private Boolean checkInputOptions(Hashtable checkOptions, String value)
        {
            Boolean valid = false;
            valid = checkOptions.ContainsKey(value);
            return valid;
        }

        public void validate()
        {

            if (optsEntered.Count == 0)
            {
                displayUsage();
            }
            if (optsEntered.Contains("help"))
            {
                displayUsage();
            }
            if (option_is_set("help"))
            {
                displayUsage();
            }
            ArrayList vec = getValue(builtInOpts);
            Boolean flagUsername = true;
            Boolean flagPassword = true;
            for (int i = 0; i < vec.Count; i++)
            {
                if (!optsEntered.ContainsKey(vec[i]))
                {
                    String missingArg = vec[i].ToString();
                    if (missingArg.Equals("password"))
                    {
                        flagPassword = false;
                    }
                    else if (missingArg.Equals("username"))
                    {
                        flagUsername = false;
                    }
                    else
                    {
                        Console.Write("----ERROR: " + vec[i] + " not specified \n");
                        displayUsage();
                    }
                }
            }
            vec = getValue(userOpts);
            for (int i = 0; i < vec.Count; i++)
            {
                if (!optsEntered.ContainsKey(vec[i]))
                {
                    Console.Write("----ERROR: " + vec[i] + " not specified \n");
                    displayUsage();
                }
            }

            if (!optsEntered.ContainsKey("disablesso"))
            {
                if (!optsEntered.ContainsKey("ssoUrl"))
                {
                    Console.WriteLine("Must specify the option --ssoUrl or --disablesso");
                    displayUsage();
                }
            }

            if ((optsEntered.ContainsKey("sessionfile")) &&
              ((optsEntered.ContainsKey("username")) && (optsEntered.ContainsKey("password"))))
            {
                Console.WriteLine("Must have one of command options 'sessionfile' or a 'username' and 'password' pair\n");
                displayUsage();
            }

            if (optsEntered.ContainsKey("ignorecert") || "true".Equals(Environment.GetEnvironmentVariable("VI_IGNORECERT"), StringComparison.CurrentCultureIgnoreCase))
            {
                _connection.ignoreCert = true;
            }
        }
        /*
        *taking out value of a particular key in the Hashtable
        *i.e checking for required =1 options
        */
        private ArrayList getValue(Hashtable checkOptions)
        {
            IEnumerator It = checkOptions.Keys.GetEnumerator();
            ArrayList vec = new ArrayList();
            while (It.MoveNext())
            {
                String str = It.Current.ToString();
                OptionSpec oSpec = (OptionSpec)checkOptions[str];
                if (oSpec.getOptionRequired() == 1)
                {
                    vec.Add(str);
                }
            }
            return vec;
        }
        public void displayUsage()
        {
            Console.WriteLine("Common .Net Options :");
            print_options(builtInOpts);
            Console.WriteLine("\nCommand specific options: ");
            print_options(userOpts);
            Console.Write("Press Enter Key To Exit: ");
            Console.ReadLine();
            Environment.Exit(1);
        }
        private void print_options(Hashtable Opts)
        {
            String type = "";
            String defaultVal = "";
            IEnumerator It;
            String help = "";
            ICollection generalKeys = (ICollection)Opts.Keys;
            It = generalKeys.GetEnumerator();
            while (It.MoveNext())
            {
                String keyValue = It.Current.ToString();
                OptionSpec oSpec = (OptionSpec)Opts[keyValue];
                if ((oSpec.getOptionType() != null) && (oSpec.getOptionDefault() != null))
                {
                    type = oSpec.getOptionType();
                    defaultVal = oSpec.getOptionDefault();
                    Console.WriteLine("   --" + keyValue + " < type " + type + ", default " + defaultVal + ">");
                }
                if ((oSpec.getOptionDefault() != null) && (oSpec.getOptionType() == null))
                {
                    defaultVal = oSpec.getOptionDefault();
                    Console.WriteLine("   --" + keyValue + " < default " + defaultVal + " >");
                }
                else if ((oSpec.getOptionType() != null) && (oSpec.getOptionDefault() == null))
                {
                    type = oSpec.getOptionType();
                    Console.WriteLine("   --" + keyValue + " < type " + type + " >");
                }
                else if ((oSpec.getOptionType() == null) && (oSpec.getOptionDefault() == null))
                {
                    Console.WriteLine("   --" + keyValue + " ");
                }
                help = oSpec.getOptionDesc();
                Console.WriteLine("      " + help);
            }
        }

        public Boolean option_is_set(String option)
        {
            Boolean valid = false;
            IEnumerator It = optsEntered.Keys.GetEnumerator();
            while (It.MoveNext())
            {
                String keyVal = It.Current.ToString();
                if (option.Equals(keyVal))
                {
                    valid = true;
                }
            }
            return valid;
        }

        public String get_option(String key)
        {
            if (optsEntered.ContainsKey(key))
            {
                foreach (DictionaryEntry uniEntry in optsEntered)
                {
                    if (uniEntry.Key.Equals(key) && uniEntry.Value != null)
                    {
                        return uniEntry.Value.ToString();
                    }
                    else if (uniEntry.Key.Equals(key) && uniEntry.Value == null)
                    {
                        throw new ArgumentHandlingException("Missing Value for Arguement: " + key);
                    }
                }
            }
            else if (checkInputOptions(builtInOpts, key))
            {
                IEnumerator It = builtInOpts.Keys.GetEnumerator();
                while (It.MoveNext())
                {
                    String strC = It.Current.ToString();
                    if (strC.Equals(key))
                    {
                        OptionSpec oSpec = (OptionSpec)builtInOpts[strC];
                        if (oSpec.getOptionDefault() != null)
                        {
                            String str = oSpec.getOptionDefault();
                            return str;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else if (checkInputOptions(userOpts, key))
            {
                IEnumerator It = userOpts.Keys.GetEnumerator();
                while (It.MoveNext())
                {
                    String strC = It.Current.ToString();
                    if (strC.Equals(key))
                    {
                        OptionSpec oSpec = (OptionSpec)userOpts[strC];
                        if (oSpec.getOptionDefault() != null)
                        {
                            String str = oSpec.getOptionDefault();
                            return str;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("undefined variable");
            }
            return null;
        }

        public void connect()
        {
            log.LogLine("Started ");
            try
            {
                initConnection();
                getServiceUtil().ClientConnectCredentials();
            }
            catch (Exception e)
            {
                log.LogLine("Exception running : " + getAppName());
                log.Close();
                throw e;
            }
        }
        public void connect(Cookie cookie)
        {
            log.LogLine("Started ");
            try
            {
                initConnection();
                getServiceUtil().ClientConnect(cookie);
            }
            catch (Exception e)
            {
                log.LogLine("Exception running : " + getAppName());
                log.Close();
                throw new ArgumentHandlingException("Exception running : "
                                                                       + getAppName());
            }
        }
        public void loadSession()
        {
            initConnection();
            getServiceUtil().ClientLoadSession();
        }

        public void saveSession(String fileName)
        {
            _svcUtil.ClientSaveSession(fileName);
        }
        public void disConnect()
        {
            try
            {
                getServiceUtil().ClientDisconnect();
            }
            catch (Exception e)
            {
                log.LogLine("Exception running : " + getAppName());
                log.Close();
                throw new ArgumentException("Exception running : " + getAppName());
            }
        }

        /**
        * @return name of the client application
        */
        public String getAppName()
        {
            return _cname;
        }

        /**
        * @return current log
        */
        public Log getLog()
        {
            return log;
        }

        /**
        * @return the service connection object
        */
        public SvcConnection getConnection()
        {
            return _connection;
        }

        /**
        * @return Service Util object
        */
        public ServiceUtil getServiceUtil()
        {
            return _svcUtil;
        }

        /**
        * @return web service url
        */
        public String getServiceUrl()
        {
            return get_option("url");
        }

        /**
          * @return web service url
          */
        public String getSsoServiceUrl()
        {
            return get_option("ssoUrl");
        }

        /**
        * @return web service username
        */
        public String getUsername()
        {
            return get_option("username");
        }

        /**
        * @return web service password
        */
        public String getPassword()
        {
            return get_option("password");
        }

        /**
        * @return if SSO is disabled
        */
        public Boolean isSSODisabled()
        {
            return optsEntered.ContainsKey("disablesso");
        }

        /// <summary>
        /// Returns the Hostname or IP of the url provided
        /// </summary>
        /// <returns></returns>
        public string getHostName()
        {
            return new UriBuilder(getServiceUrl()).Host;
        }

        private String readUserName()
        {
            Console.Write("Enter Username : ");
            String username = Console.ReadLine();
            return username;
        }

        private String readPassword()
        {
            char[] data = new Char[50];
            Console.Write("Enter Password : ");
            ConsoleKeyInfo key = Console.ReadKey(true);
            int i = 0;
            while (key.KeyChar != '\r')
            {
                data[i] = key.KeyChar;
                key = Console.ReadKey(true);
                i++;
            }
            return new String(data, 0, i); ;
        }

    }
}
