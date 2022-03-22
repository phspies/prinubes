using PlatformWorker.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;


public static class Utils
{

    public static Guid NewGuid()
    {
        return Guid.NewGuid();
    }
    public static string NewSHA1()
    {
        using (SHA1Managed sha1 = new SHA1Managed())
        {
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(Utils.NewGuid().ToString()));
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }

    public static string BuildFileName(string[] parts, string suffix)
    {
        StringBuilder stringBuilder = new StringBuilder(parts[0]);
        for (int index = 1; index < parts.Length; ++index)
        {
            stringBuilder.Append("\\");
            stringBuilder.Append(parts[index]);
        }
        stringBuilder.Append(suffix);
        return stringBuilder.ToString();
    }

    public static string[] GetLocalAvailableDriveLetters()
    {
        return ((IEnumerable<string>)new string[24] { "C:\\", "D:\\", "E:\\", "F:\\", "G:\\", "H:\\", "I:\\", "J:\\", "K:\\", "L:\\", "M:\\", "N:\\", "O:\\", "P:\\", "Q:\\", "R:\\", "S:\\", "T:\\", "U:\\", "V:\\", "W:\\", "X:\\", "Y:\\", "Z:\\" }).Except<string>((IEnumerable<string>)Environment.GetLogicalDrives()).ToArray<string>();
    }

    public static T[] CollectionToArray<T>(ICollection<T> c)
    {
        if (c == null || c.Count == 0)
            return new T[0];
        T[] array = new T[c.Count];
        c.CopyTo(array, 0);
        return array;
    }

    public static void AddOrReplace<K, V>(IDictionary<K, V> data, K key, V value)
    {
        if (data.ContainsKey(key))
            data[key] = value;
        else
            data.Add(key, value);
    }

    public static void AddOrReplace(IDictionary data, object key, object value)
    {
        if (data.Contains(key))
            data[key] = value;
        else
            data.Add(key, value);
    }

    public static void AddOrReplace<V>(ICollection<V> data, V value)
    {
        if (data.Contains(value))
            return;
        data.Add(value);
    }

    public static bool IsInteractive()
    {
        Thread.GetDomain().SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
        return (Thread.CurrentPrincipal as WindowsPrincipal).IsInRole(new SecurityIdentifier(WellKnownSidType.InteractiveSid, (SecurityIdentifier)null));
    }

    public static string GetGuestOS(OperatingSystemInfo osInfo)
    {
        string str = (string)null;
        if (5 == osInfo.Version.Major)
            str = osInfo.Version.Minor != 0 ? (1 != osInfo.Version.Minor ? ((osInfo.ProductSuite & 2) != 2 ? (osInfo.Architecture != OperatingSystemArchitecture.x86 ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "winNetStandard64Guest") : "winNetStandardGuest") : (osInfo.Architecture != OperatingSystemArchitecture.x86 ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "winNetEnterprise64Guest") : "winNetEnterpriseGuest")) : (osInfo.Architecture != OperatingSystemArchitecture.x86 ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "winXPPro64Guest") : "winXPProGuest")) : ((osInfo.ProductSuite & 2) != 2 ? "win2000ServGuest" : "win2000AdvServGuest");
        else if (6 == osInfo.Version.Major)
            str = osInfo.Version.Minor != 0 ? (1 != osInfo.Version.Minor ? (osInfo.ProductType != OperatingSystemProductType.Workstation ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "windows8Server64Guest") : (osInfo.Architecture != OperatingSystemArchitecture.x86 ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "windows8_64Guest") : "windows8Guest")) : (osInfo.ProductType != OperatingSystemProductType.Workstation ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "windows7Server64Guest") : (osInfo.Architecture != OperatingSystemArchitecture.x86 ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "windows7_64Guest") : "windows7Guest"))) : (osInfo.ProductType != OperatingSystemProductType.Workstation ? (osInfo.Architecture != OperatingSystemArchitecture.x86 ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "winLonghorn64Guest") : "winLonghornGuest") : (osInfo.Architecture != OperatingSystemArchitecture.x86 ? (osInfo.Architecture != OperatingSystemArchitecture.x64 ? "NotSupported" : "winVista64Guest") : "winVistaGuest"));
        return str;
    }

    public static OperatingSystemInfo GetOsInfoFromGuestId(string guestOS)
    {
        if (string.IsNullOrEmpty(guestOS))
            return (OperatingSystemInfo)null;
        OperatingSystemInfo operatingSystemInfo = new OperatingSystemInfo();
        operatingSystemInfo.ServicePack = string.Empty;
        if (string.Compare(guestOS, "win2000AdvServGuest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 0
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x86;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        if (string.Compare(guestOS, "win2000ServGuest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 0
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x86;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "winXPProGuest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 1
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x86;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Workstation;
        }
        else if (string.Compare(guestOS, "winXPPro64Guest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 1
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x64;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "winNetEnterpriseGuest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 2
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x86;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "winNetEnterprise64Guest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 2
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x64;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "winNetStandardGuest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 2
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x86;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "winNetStandard64Guest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 5,
                Minor = 2
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x64;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "winVistaGuest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 6,
                Minor = 0
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x86;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Workstation;
        }
        else if (string.Compare(guestOS, "winVista64Guest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 6,
                Minor = 0
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x64;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Workstation;
        }
        else if (string.Compare(guestOS, "winLonghornGuest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 6,
                Minor = 0
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x86;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "winLonghorn64Guest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 6,
                Minor = 0
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x64;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "windows7Server64Guest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 6,
                Minor = 1
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x64;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        else if (string.Compare(guestOS, "windows8Server64Guest", true, CultureInfo.InvariantCulture) == 0)
        {
            operatingSystemInfo.Version = new OperatingSystemVersion()
            {
                Major = 6,
                Minor = 2
            };
            operatingSystemInfo.VersionString = operatingSystemInfo.Version.ToString();
            operatingSystemInfo.Architecture = OperatingSystemArchitecture.x64;
            operatingSystemInfo.ProductType = OperatingSystemProductType.Server;
        }
        return operatingSystemInfo;
    }

   
    public static void GetShareInfoFromUnc(string sharePath, out string serverName, out string shareName)
    {
        serverName = string.Empty;
        shareName = string.Empty;
        if (!Utils.IsUnc(sharePath))
            return;
        string[] strArray = sharePath.Substring(2).Split('\\');
        if (((IEnumerable<string>)strArray).Count<string>() < 3)
            return;
        serverName = strArray[0];
        shareName = strArray[1];
    }

    public static string GetSharePathFromUnc(string fullPath)
    {
        string serverName = string.Empty;
        string shareName = string.Empty;
        Utils.GetShareInfoFromUnc(fullPath, out serverName, out shareName);
        if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(shareName))
            return string.Empty;
        return string.Format("\\\\{0}\\{1}", (object)serverName, (object)shareName);
    }

    public static bool IsUnc(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        return path.StartsWith("\\\\");
    }

    public static bool CheckWriteAccessToShare(string path, string fileServer, string shareName)
    {
        try
        {
            if (Directory.Exists(path))
            {
                string path1 = Path.Combine(path, new Guid().ToString());
                string contents = "test";
                System.IO.File.WriteAllText(path1, contents);
                System.IO.File.Delete(path1);
            }
            else
            {
                Directory.CreateDirectory(path);
                string path1 = Path.Combine(path, new Guid().ToString());
                string contents = "test";
                System.IO.File.WriteAllText(path1, contents);
                System.IO.File.Delete(path1);
                string str = path.TrimEnd("\\".ToCharArray());
                string b = "\\\\" + fileServer + "\\" + shareName;
                try
                {
                    for (; !string.Equals(str, b); str = str.Substring(0, str.LastIndexOf("\\")).TrimEnd("\\".ToCharArray()))
                        Directory.Delete(str);
                }
                catch
                {
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool PingServer(string serverName, int numberOfAttempts)
    {
        bool flag = false;
        for (int index = 0; index < numberOfAttempts; ++index)
        {
            if (Utils.PingServer(serverName))
            {
                flag = true;
                break;
            }
            Thread.Sleep(100);
        }
        return flag;
    }

    public static bool PingServer(string serverName)
    {
        bool flag = false;
        try
        {
            using (Ping ping = new Ping())
                flag = ping.Send(serverName).Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
        }
        return flag;
    }

    public static void WaitForReboot(string serverName, int interval, int timeout)
    {
        int num1 = 0;
        int num2 = 0;
        bool flag = false;
        while (true)
        {
            if (!Utils.PingServer(serverName))
            {
                ++num2;
                if (!flag && num2 > 3)
                    flag = true;
            }
            else if (flag)
                break;
            if (timeout <= 0 || num1 <= timeout)
            {
                Thread.Sleep(interval * 1000);
                num1 += interval;
            }
            else
                goto label_6;
        }
        return;
        label_6:
        throw new Exception("Timeout occurred while waiting for server " + serverName + " to reboot");
    }

    public static StringBuilder EscapeSpecialChars(string value)
    {
        StringBuilder stringBuilder = new StringBuilder(value);
        string oldValue1 = " ";
        string newValue1 = "\\ ";
        stringBuilder.Replace(oldValue1, newValue1);
        string oldValue2 = "&";
        string newValue2 = "\\&";
        stringBuilder.Replace(oldValue2, newValue2);
        string oldValue3 = "$";
        string newValue3 = "\\$";
        stringBuilder.Replace(oldValue3, newValue3);
        string oldValue4 = "(";
        string newValue4 = "\\(";
        stringBuilder.Replace(oldValue4, newValue4);
        string oldValue5 = ")";
        string newValue5 = "\\)";
        stringBuilder.Replace(oldValue5, newValue5);
        return stringBuilder;
    }

    public static long RoundUp(long mb)
    {
        return mb + (4L - mb % 4L) % 4L;
    }

    public static void SplitUsernameAndDomain(string fullUsername, out string username, out string domain)
    {
        if (fullUsername.Contains("\\"))
        {
            string[] strArray = fullUsername.Split("\\".ToCharArray());
            username = strArray[1];
            domain = strArray[0];
        }
        else if (fullUsername.Contains("@"))
        {
            string[] strArray = fullUsername.Split("@".ToCharArray());
            username = strArray[0];
            domain = strArray[1];
        }
        else
        {
            username = fullUsername;
            domain = string.Empty;
        }
    }

    public static string CombinServerNameAndPort(string server, int port)
    {
        IPAddress address;
        server = !IPAddress.TryParse(server, out address) ? server + ":" + (object)port : IPHelper.IpBracketed(address.ToString()) + ":" + (object)port;
        return server;
    }

    public static string CombinUsernameAndDomain(string username, string domain)
    {
        string str = username;
        if (!string.IsNullOrEmpty(domain))
            str = domain + "\\" + username;
        return str;
    }

    public static string GetFullUserName(NetworkCredential credentials)
    {
        if (credentials == null)
            return string.Empty;
        string str = credentials.UserName;
        if (!string.IsNullOrEmpty(credentials.Domain))
            str = credentials.Domain + "\\" + credentials.UserName;
        return str;
    }

    public static int CompareExecutableVersions(string f1, string f2)
    {
        FileVersionInfo versionInfo1 = FileVersionInfo.GetVersionInfo(f1);
        FileVersionInfo versionInfo2 = FileVersionInfo.GetVersionInfo(f2);
        if (versionInfo1.ProductMajorPart > versionInfo2.ProductMajorPart)
            return 1;
        if (versionInfo1.ProductMajorPart < versionInfo2.ProductMajorPart)
            return -1;
        if (versionInfo1.ProductMinorPart > versionInfo2.ProductMinorPart)
            return 1;
        if (versionInfo1.ProductMinorPart < versionInfo2.ProductMinorPart)
            return -1;
        if (versionInfo1.ProductBuildPart > versionInfo2.ProductBuildPart)
            return 1;
        if (versionInfo1.ProductBuildPart < versionInfo2.ProductBuildPart)
            return -1;
        if (versionInfo1.ProductPrivatePart > versionInfo2.ProductPrivatePart)
            return 1;
        return versionInfo1.ProductPrivatePart < versionInfo2.ProductPrivatePart ? -1 : 0;
    }

    public static int CompareVersions(string v1, string v2)
    {
        if (string.IsNullOrEmpty(v1) && string.IsNullOrEmpty(v2))
            return 0;
        if (string.IsNullOrEmpty(v1) && !string.IsNullOrEmpty(v2))
            return -1;
        if (!string.IsNullOrEmpty(v1) && string.IsNullOrEmpty(v2))
            return 1;
        string[] strArray1 = v1.Split(".".ToCharArray());
        string[] strArray2 = v2.Split(".".ToCharArray());
        for (int index = 0; index < strArray1.Length; ++index)
        {
            int int32_1 = Convert.ToInt32(strArray1[index]);
            if (strArray2.Length <= index)
                return 1;
            int int32_2 = Convert.ToInt32(strArray2[index]);
            if (int32_1 > int32_2)
                return 1;
            if (int32_1 < int32_2)
                return -1;
        }
        return strArray1.Length == strArray2.Length ? 0 : -1;
    }

    public static bool IsInternetConnectionExists()
    {
        try
        {
            new TcpClient("www.google.com", 80).Close();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static int RunShellCmd(string cmd, string args, out string stdout, out string stderr)
    {
        return Utils.RunShellCmd(cmd, args, int.MaxValue, out stdout, out stderr);
    }

    public static int RunShellCmd(string cmd, string args, int maxwait, out string stdout, out string stderr)
    {
        if (!((IEnumerable<string>)new string[3] { "zh", "ja", "ko" }).Contains<string>(CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
            return Utils.RunShellCmdStdOut(cmd, args, maxwait, out stdout, out stderr);
        int num = Utils.RunShellCmdTimeout(cmd, args, maxwait);
        stdout = "";
        stderr = "";
        return num;
    }

    private static int RunShellCmdStdOut(string cmd, string args, int maxwait, out string stdout, out string stderr)
    {
        object stdOutMutex = new object();
        object stdErrMutex = new object();
        using (Process process1 = new Process())
        {
            process1.StartInfo.UseShellExecute = false;
            process1.StartInfo.FileName = cmd;
            process1.StartInfo.Arguments = args;
            StringWriter tmpStdOut = new StringWriter();
            try
            {
                StringWriter tmpStdErr = new StringWriter();
                try
                {
                    lock (stdOutMutex)
                        tmpStdOut.WriteLine("Executing:  {0} {1}", (object)cmd, (object)args);
                    process1.StartInfo.RedirectStandardOutput = true;
                    DataReceivedEventHandler receivedEventHandler1 = (DataReceivedEventHandler)((sendingProcess, outLine) =>
                   {
                       if (string.IsNullOrEmpty(outLine.Data))
                           return;
                       try
                       {
                           lock (stdOutMutex)
                               tmpStdOut.WriteLine("StdOut({0}): {1}", (object)cmd, (object)outLine.Data);
                       }
                       catch (Exception ex)
                       {
                       }
                   });
                    process1.OutputDataReceived += receivedEventHandler1;
                    process1.StartInfo.RedirectStandardError = true;
                    DataReceivedEventHandler receivedEventHandler2 = (DataReceivedEventHandler)((sendingProcess, outLine) =>
                   {
                       if (string.IsNullOrEmpty(outLine.Data))
                           return;
                       Process process = (Process)sendingProcess;
                       if (process != null)
                       {
                           string fileName = process.StartInfo.FileName;
                       }
                       try
                       {
                           lock (stdErrMutex)
                               tmpStdErr.WriteLine("StdErr({0}): {1}", (object)cmd, (object)outLine.Data);
                       }
                       catch (Exception ex)
                       {
                       }
                   });
                    process1.ErrorDataReceived += receivedEventHandler2;
                    try
                    {
                        process1.Start();
                        process1.BeginOutputReadLine();
                        process1.BeginErrorReadLine();
                        if (!process1.WaitForExit(maxwait))
                        {
                            process1.Kill();
                            process1.WaitForExit(30000);
                            lock (stdOutMutex)
                                stdout = tmpStdOut.ToString();
                            lock (stdErrMutex)
                                stderr = tmpStdErr.ToString();
                            throw new System.TimeoutException();
                        }
                        lock (stdOutMutex)
                            stdout = tmpStdOut.ToString();
                        lock (stdErrMutex)
                            stderr = tmpStdErr.ToString();
                    }
                    finally
                    {
                        process1.OutputDataReceived -= receivedEventHandler1;
                        process1.ErrorDataReceived -= receivedEventHandler2;
                    }
                }
                finally
                {
                    if (tmpStdErr != null)
                        tmpStdErr.Dispose();
                }
            }
            finally
            {
                if (tmpStdOut != null)
                    tmpStdOut.Dispose();
            }
            return process1.ExitCode;
        }
    }

    public static int RunShellCmdLog(string cmd, string args, int maxwait)
    {
        using (Process process1 = new Process())
        {
            process1.StartInfo.UseShellExecute = false;
            process1.StartInfo.FileName = cmd;
            process1.StartInfo.Arguments = args;
            process1.StartInfo.RedirectStandardOutput = true;
            DataReceivedEventHandler receivedEventHandler1 = (DataReceivedEventHandler)((sendingProcess, outLine) =>
           {
               if (string.IsNullOrEmpty(outLine.Data))
                   return;
           });
            process1.OutputDataReceived += receivedEventHandler1;
            process1.StartInfo.RedirectStandardError = true;
            DataReceivedEventHandler receivedEventHandler2 = (DataReceivedEventHandler)((sendingProcess, outLine) =>
           {
               if (string.IsNullOrEmpty(outLine.Data))
                   return;
               Process process = (Process)sendingProcess;
               if (process != null)
               {
                   string fileName = process.StartInfo.FileName;
               }
           });
            process1.ErrorDataReceived += receivedEventHandler2;
            try
            {
                process1.Start();
                process1.BeginOutputReadLine();
                process1.BeginErrorReadLine();
                if (!process1.WaitForExit(maxwait))
                {
                    process1.Kill();
                    process1.WaitForExit(30000);
                    throw new System.TimeoutException();
                }
            }
            finally
            {
                process1.OutputDataReceived -= receivedEventHandler1;
                process1.ErrorDataReceived -= receivedEventHandler2;
            }
            return process1.ExitCode;
        }
    }

    public static int RunShellCmd(string cmd, string args)
    {
        return Utils.RunShellCmdTimeout(cmd, args, int.MaxValue);
    }

    public static int RunShellCmdTimeout(string cmd, string args, int maxwait)
    {
        using (Process process = new Process())
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = args;
            process.Start();
            if (!process.WaitForExit(maxwait))
            {
                process.Kill();
                process.WaitForExit(30000);
                throw new System.TimeoutException();
            }
            return process.ExitCode;
        }
    }

    public static void CopyFolder(string sourceFolder, string destFolder)
    {
        if (!Directory.Exists(destFolder))
            Directory.CreateDirectory(destFolder);
        foreach (string file in Directory.GetFiles(sourceFolder))
        {
            string fileName = Path.GetFileName(file);
            string destFileName = Path.Combine(destFolder, fileName);
            int num = 1;
            System.IO.File.Copy(file, destFileName, num != 0);
        }
        foreach (string directory in Directory.GetDirectories(sourceFolder))
        {
            string fileName = Path.GetFileName(directory);
            string destFolder1 = Path.Combine(destFolder, fileName);
            Utils.CopyFolder(directory, destFolder1);
        }
    }

    public static void SplitServerNameAndPort(string server, out string serverName, out int port)
    {
        serverName = server;
        port = 0;
        if (server.Split(':').Length >= 3)
        {
            if (!server.Contains("[") || !server.Contains("]"))
                return;
            string[] strArray1 = server.Split("[]".ToCharArray());
            serverName = strArray1[1];
            if (!strArray1[2].Contains(":"))
                return;
            string[] strArray2 = strArray1[2].Split(':');
            try
            {
                port = int.Parse(strArray2[1]);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Invalid port format");
            }
        }
        else
        {
            string[] strArray = server.Split(':');
            serverName = strArray[0];
            if (strArray.Length != 2)
                return;
            try
            {
                port = int.Parse(strArray[1]);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Invalid port format");
            }
        }
    }

    public static string TrimDomainFromFQDN(string server)
    {
        string str = server;
        IPAddress address;
        if (!IPAddress.TryParse(server, out address) && server.Contains("."))
            str = server.Remove(server.IndexOf("."));
        return str;
    }

    public static void UnprotectFile(string fileName)
    {
        if (!System.IO.File.Exists(fileName))
            return;
        FileAttributes fileAttributes = System.IO.File.GetAttributes(fileName) & ~FileAttributes.Hidden & ~FileAttributes.System & ~FileAttributes.ReadOnly;
        System.IO.File.SetAttributes(fileName, fileAttributes);
    }

    public static bool IsSameFolder(string path1, string path2)
    {
        if (!Directory.Exists(path1) || !Directory.Exists(path2))
            throw new ArgumentException("Invalid arguments");
        string path2_1 = Utils.NewGuid().ToString();
        bool flag = false;
        string path = Path.Combine(path1, path2_1);
        try
        {
            Directory.CreateDirectory(path);
            flag = Directory.Exists(Path.Combine(path2, path2_1));
        }
        catch (Exception ex)
        {
        }
        finally
        {
            Directory.Delete(path);
        }
        return flag;
    }

    public static void Retry(int attempts, int Wait, Utils.Workload work, Utils.Workload reset)
    {
        Exception exception = (Exception)null;
        for (int index = 0; index < attempts; ++index)
        {
            try
            {
                work();
                return;
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = ex;
            }
            reset();
            Thread.Sleep(Wait);
        }
        throw exception;
    }

    public static void Retry(int attempts, int Wait, Utils.Workload work)
    {
        Exception exception = (Exception)null;
        for (int index = 0; index < attempts; ++index)
        {
            try
            {
                work();
                return;
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = ex;
            }
            Thread.Sleep(Wait);
        }
        throw exception;
    }

    public static void RetryIf(int attempts, int Wait, Utils.Workload work, Func<Exception, bool> cond)
    {
        Exception exception = (Exception)null;
        bool flag = true;
        int num = 0;
        while (flag)
        {
            if (num < attempts)
            {
                try
                {
                    work();
                    return;
                }
                catch (Exception ex)
                {
                    flag = cond(ex);
                    if (exception == null)
                        exception = ex;
                }
                Thread.Sleep(Wait);
                ++num;
            }
            else
                break;
        }
        throw exception;
    }

    public static T Retry<T>(int attempts, int Wait, Func<T> work, Utils.Workload reset)
    {
        Exception exception = (Exception)null;
        for (int index = 0; index < attempts; ++index)
        {
            try
            {
                return work();
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = ex;
            }
            reset();
            Thread.Sleep(Wait);
        }
        throw exception;
    }

    public static T Retry<T>(int attempts, int Wait, Func<T> work)
    {
        Exception exception = (Exception)null;
        for (int index = 0; index < attempts; ++index)
        {
            try
            {
                return work();
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = ex;
            }
            Thread.Sleep(Wait);
        }
        throw exception;
    }

    public static T RetryIf<T>(int attempts, int Wait, Func<T> work, Func<Exception, bool> cond)
    {
        Exception exception = (Exception)null;
        bool flag = true;
        int num = 0;
        while (flag)
        {
            if (num < attempts)
            {
                try
                {
                    return work();
                }
                catch (Exception ex)
                {
                    flag = cond(ex);
                    exception = ex;
                }
                Thread.Sleep(Wait);
                ++num;
            }
            else
                break;
        }
        throw exception;
    }

    public static bool WaitForResult(int attempts, int Wait, Func<bool> work)
    {
        Exception exception = (Exception)null;
        for (int index = 0; index < attempts; ++index)
        {
            try
            {
                if (work())
                    return true;
                exception = (Exception)null;
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = ex;
            }
            Thread.Sleep(Wait);
        }
        if (exception != null)
            throw exception;
        return false;
    }

    public static void TryLockRun(object lck, Utils.Workload w)
    {
        if (!Monitor.TryEnter(lck))
            return;
        try
        {
            w();
        }
        finally
        {
            Monitor.Exit(lck);
        }
    }

    public static string GetDirectoryName(string fullName)
    {
        int length = fullName.LastIndexOf('/');
        if (length != -1)
            return fullName.Substring(0, length);
        return "";
    }

  
    public static bool ContainsDigit(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        bool flag = false;
        value.ToCharArray();
        foreach (char c in value.ToCharArray())
        {
            if (char.IsDigit(c))
            {
                flag = true;
                break;
            }
        }
        return flag;
    }

    public static bool CompareEsxVmUuidAndBiosUuid(string vmUuid, string biosUuid)
    {
        if (string.IsNullOrEmpty(vmUuid) || string.IsNullOrEmpty(biosUuid))
            return false;
        string[] strArray = vmUuid.Split("-".ToCharArray());
        for (int index1 = 0; index1 < 3; ++index1)
        {
            string str1 = strArray[index1];
            string str2 = "";
            int index2 = str1.Length - 1;
            while (index2 >= 0)
            {
                string str3 = str2;
                char ch = str1[index2 - 1];
                string str4 = ch.ToString();
                string str5 = str3 + str4;
                ch = str1[index2];
                string str6 = ch.ToString();
                str2 = str5 + str6;
                index2 -= 2;
            }
            strArray[index1] = str2;
        }
        vmUuid = string.Join("-", strArray);
        return string.Compare(vmUuid, biosUuid, true) == 0;
    }

    public static string GetMD5(string fileName)
    {
        FileStream fileStream = (FileStream)null;
        MD5CryptoServiceProvider cryptoServiceProvider = (MD5CryptoServiceProvider)null;
        try
        {
            if (!System.IO.File.Exists(fileName))
                throw new FileNotFoundException("File not found", fileName);
            fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            cryptoServiceProvider = new MD5CryptoServiceProvider();
            cryptoServiceProvider.ComputeHash((Stream)fileStream);
            return Utils.BytesToStr(cryptoServiceProvider.Hash);
        }
        finally
        {
            if (fileStream != null)
                fileStream.Close();
            if (cryptoServiceProvider != null)
                cryptoServiceProvider.Clear();
        }
    }

    public static string BytesToStr(byte[] bytes)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < bytes.Length; ++index)
            stringBuilder.AppendFormat("{0:X2}", (object)bytes[index]);
        return stringBuilder.ToString();
    }

    public static bool IsWin2K8R1(OperatingSystemInfo osInfo)
    {
        if (osInfo.Version.Major == 6)
            return osInfo.Version.Minor == 0;
        return false;
    }

    public static string GetBootIniDefaultConfig(string filename)
    {
        string[] strArray1 = new string[1] { "" };
        string[] strArray2 = System.IO.File.ReadAllLines(filename);
        string DefaultLocation = "";
        DefaultLocation = ((IEnumerable<string>)strArray2).Single<string>((Func<string, bool>)(x => x.Contains("default")));
        DefaultLocation = DefaultLocation.Substring(DefaultLocation.IndexOf("=") + 1);
        string str1 = ((IEnumerable<string>)strArray2).First<string>((Func<string, bool>)(x => x.StartsWith(DefaultLocation)));
        string str2 = "=";
        int startIndex = str1.IndexOf(str2) + 1;
        return str1.Substring(startIndex);
    }

    public static List<T> ToList<T>(this T[] array)
    {
        if (array == null)
            throw new ArgumentNullException();
        List<T> objList = new List<T>(array.Length);
        foreach (T obj in array)
            objList.Add(obj);
        return objList;
    }

    public static string GetEsxVolumeName(string volName)
    {
        return string.IsNullOrEmpty(volName) ? "[Local] " : "[" + volName + "] ";
    }

    public static string BuildEsxDiskName(string serverName, string diskName)
    {
        return serverName + "_" + diskName + ".vmdk";
    }

    public static bool IsWin2K8R2(OperatingSystemInfo osInfo)
    {
        if (osInfo.Version.Major == 6)
            return osInfo.Version.Minor == 1;
        return false;
    }

    public static bool IsW2K8R2SupportedOnEsx(string esxVersion, string buildNum)
    {
        bool flag = true;
        ulong uint64 = Convert.ToUInt64(buildNum);
        if (Utils.CompareVersions("3.5.0", esxVersion) == 0)
        {
            if (uint64 < 207095UL)
                flag = false;
        }
        else if (Utils.CompareVersions("4.0.0", esxVersion) == 0 && uint64 < 208167UL)
            flag = false;
        return flag;
    }

    public static string GetVimUuidFromBiosUuid(string vmUuid)
    {
        if (string.IsNullOrEmpty(vmUuid))
            return (string)null;
        string[] strArray = vmUuid.Split("-".ToCharArray());
        for (int index1 = 0; index1 < 3; ++index1)
        {
            string str1 = strArray[index1];
            string str2 = "";
            int index2 = str1.Length - 1;
            while (index2 >= 0)
            {
                string str3 = str2;
                char ch = str1[index2 - 1];
                string str4 = ch.ToString();
                string str5 = str3 + str4;
                ch = str1[index2];
                string str6 = ch.ToString();
                str2 = str5 + str6;
                index2 -= 2;
            }
            strArray[index1] = str2;
        }
        vmUuid = string.Join("-", strArray);
        return vmUuid;
    }

    public static string GetVimUuidFromBiosUuid(Guid uuid)
    {
        if (uuid == Guid.Empty)
            return (string)null;
        return Utils.GetVimUuidFromBiosUuid(uuid.ToString());
    }

    public static bool DiskPart(string script, object diskPartLock, int timeoutMilliseconds)
    {
        lock (diskPartLock)
        {
            string local_2 = Path.GetTempFileName();
            using (StreamWriter resource_0 = new StreamWriter(local_2))
                resource_0.WriteLine(script);
            using (StreamReader resource_1 = new StreamReader(local_2))
            {
                while (resource_1.Peek() >= 0)
                {

                }
            }
            string local_3 = string.Format("/s \"{0}\"", (object)local_2);
            string local_4 = string.Empty;
            string local_5 = string.Empty;
            try
            {
                int local_8 = 999;
                for (int local_9 = 0; local_8 != 0 && local_9 < 3; ++local_9)
                {

                    local_8 = Utils.RunShellCmdLog("diskpart", local_3, timeoutMilliseconds);
                    Thread.Sleep(15000);
                    if (local_8 == 4)
                    {
                        ServiceController local_10 = ((IEnumerable<ServiceController>)ServiceController.GetServices()).First<ServiceController>((Func<ServiceController, bool>)(sc => string.Compare(sc.ServiceName, "vds", true) == 0));
                        if (local_10 == null)
                            throw new Exception("Virtual Disk Service not found");
                        if (local_10.Status != ServiceControllerStatus.Running)
                        {
                            local_10.Start();
                        }
                    }
                }
                return local_8 == 0;
            }
            catch (System.TimeoutException exception_0)
            {
                return false;
            }
            catch (Exception exception_1)
            {
                return false;
            }
            finally
            {
                for (int local_13 = 0; local_13 < 3; ++local_13)
                {
                    try
                    {
                        System.IO.File.Delete(local_2);
                        break;
                    }
                    catch (Exception exception_2)
                    {
                        Thread.Sleep(5000);
                    }
                }
            }
        }
    }


    public static void GetUsernameAndDomain(string username, string domain, out string usernamePart, out string domainPart)
    {
        usernamePart = string.Empty;
        domainPart = string.Empty;
        if (string.IsNullOrEmpty(domain))
        {
            Utils.SplitUsernameAndDomain(username, out usernamePart, out domainPart);
        }
        else
        {
            usernamePart = username;
            domainPart = domain;
        }
    }

    public delegate void Workload();
}
