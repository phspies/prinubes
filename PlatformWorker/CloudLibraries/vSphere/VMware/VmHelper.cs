using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    internal class VmHelper
    {
        internal static VirtualMachineConfigSpec CreateVmConfigSpec(VirtualMachineConfigInfo srcVmCfgInfo, string targetDatastore, string replicaDisplayName)
        {
            VirtualMachineConfigSpec machineConfigSpec = new VirtualMachineConfigSpec();
            machineConfigSpec.alternateGuestName = srcVmCfgInfo.alternateGuestName;
            machineConfigSpec.annotation = srcVmCfgInfo.annotation;
            machineConfigSpec.bootOptions = srcVmCfgInfo.bootOptions;
            machineConfigSpec.changeTrackingEnabled = false;
            machineConfigSpec.changeTrackingEnabledSpecified = true;
            machineConfigSpec.consolePreferences = srcVmCfgInfo.consolePreferences;
            machineConfigSpec.cpuAffinity = srcVmCfgInfo.cpuAffinity;
            machineConfigSpec.cpuFeatureMask = VmHelper.GetCpuFeatureMask(srcVmCfgInfo.cpuFeatureMask);
            machineConfigSpec.extraConfig = VmHelper.GetExtraConfig(srcVmCfgInfo.extraConfig);
            machineConfigSpec.files = new VirtualMachineFileInfo()
            {
                vmPathName = VmHelper.GetBracketedName(targetDatastore),
                snapshotDirectory = VmHelper.GetBracketedName(targetDatastore)
            };
            machineConfigSpec.flags = srcVmCfgInfo.flags;
            machineConfigSpec.ftInfo = srcVmCfgInfo.ftInfo;
            machineConfigSpec.guestId = srcVmCfgInfo.guestId;
            machineConfigSpec.memoryAffinity = srcVmCfgInfo.memoryAffinity;
            machineConfigSpec.memoryMB = srcVmCfgInfo.hardware.memoryMB;
            machineConfigSpec.memoryMBSpecified = true;
            machineConfigSpec.name = replicaDisplayName;
            machineConfigSpec.networkShaper = srcVmCfgInfo.networkShaper;
            machineConfigSpec.npivWorldWideNameType = srcVmCfgInfo.npivWorldWideNameType;
            machineConfigSpec.numCPUs = srcVmCfgInfo.hardware.numCPU;
            machineConfigSpec.numCPUsSpecified = true;
            machineConfigSpec.powerOpInfo = srcVmCfgInfo.defaultPowerOps;
            machineConfigSpec.swapPlacement = srcVmCfgInfo.swapPlacement;
            machineConfigSpec.tools = srcVmCfgInfo.tools;
            machineConfigSpec.vAppConfig = VmHelper.VmConfigInfoToVmConfigSpec(srcVmCfgInfo.vAppConfig);
            machineConfigSpec.vAssertsEnabled = srcVmCfgInfo.vAssertsEnabled;
            machineConfigSpec.vAssertsEnabledSpecified = true;
            machineConfigSpec.version = srcVmCfgInfo.version;
            machineConfigSpec.deviceChange = VmHelper.CreateVmDeviceSpecs(srcVmCfgInfo.hardware);
            if (srcVmCfgInfo.version != "vmx-04")
            {
                machineConfigSpec.cpuHotAddEnabled = srcVmCfgInfo.cpuHotAddEnabled;
                machineConfigSpec.cpuHotAddEnabledSpecified = true;
                machineConfigSpec.cpuHotRemoveEnabled = srcVmCfgInfo.cpuHotRemoveEnabled;
                machineConfigSpec.cpuHotRemoveEnabledSpecified = true;
                machineConfigSpec.memoryHotAddEnabled = srcVmCfgInfo.memoryHotAddEnabled;
                machineConfigSpec.memoryHotAddEnabledSpecified = true;
                machineConfigSpec.npivDesiredNodeWwns = srcVmCfgInfo.npivDesiredNodeWwns;
                machineConfigSpec.npivDesiredNodeWwnsSpecified = true;
                machineConfigSpec.npivDesiredPortWwns = srcVmCfgInfo.npivDesiredPortWwns;
                machineConfigSpec.npivDesiredPortWwnsSpecified = true;
                machineConfigSpec.npivNodeWorldWideName = srcVmCfgInfo.npivNodeWorldWideName;
                machineConfigSpec.npivOnNonRdmDisks = srcVmCfgInfo.npivOnNonRdmDisks;
                machineConfigSpec.npivOnNonRdmDisksSpecified = true;
                machineConfigSpec.npivPortWorldWideName = srcVmCfgInfo.npivPortWorldWideName;
                machineConfigSpec.npivTemporaryDisabled = srcVmCfgInfo.npivTemporaryDisabled;
                machineConfigSpec.npivTemporaryDisabledSpecified = true;
            }
            return machineConfigSpec;
        }

        private static VirtualDeviceConfigSpec[] CreateVmDeviceSpecs(VirtualHardware virtualHardware)
        {
            List<VirtualDeviceConfigSpec> deviceConfigSpecList = new List<VirtualDeviceConfigSpec>();
            foreach (VirtualDevice virtualDevice in virtualHardware.device)
            {
                if (!(virtualDevice is VirtualDisk))
                {
                    if (virtualDevice is VirtualFloppy)
                    {
                        VirtualFloppyRemoteDeviceBackingInfo deviceBackingInfo = new VirtualFloppyRemoteDeviceBackingInfo();
                        deviceBackingInfo.deviceName = "";
                        virtualDevice.backing = deviceBackingInfo;
                    }
                    else if (virtualDevice is VirtualCdrom)
                    {
                        VirtualCdromRemotePassthroughBackingInfo passthroughBackingInfo = new VirtualCdromRemotePassthroughBackingInfo();
                        passthroughBackingInfo.deviceName = "";
                        passthroughBackingInfo.exclusive = false;
                        virtualDevice.backing = passthroughBackingInfo;
                    }
                    deviceConfigSpecList.Add(new VirtualDeviceConfigSpec()
                    {
                        operation = VirtualDeviceConfigSpecOperation.add,
                        operationSpecified = true,
                        device = virtualDevice
                    });
                }
            }
            return deviceConfigSpecList.ToArray();
        }

        private static OptionValue[] GetExtraConfig(OptionValue[] srcExtraConfig)
        {
            List<OptionValue> optionValueList = new List<OptionValue>();
            foreach (OptionValue optionValue in srcExtraConfig)
            {
                if (string.Compare(optionValue.key, "sched.swap.derivedName", true) != 0 && !optionValue.key.Contains("CPUID") && !optionValue.key.Contains("ctkEnabled"))
                    optionValueList.Add(optionValue);
            }
            return optionValueList.ToArray();
        }

        private static VirtualMachineCpuIdInfoSpec[] GetCpuFeatureMask(HostCpuIdInfo[] srcCpuFeatureMask)
        {
            if (srcCpuFeatureMask == null)
                return null;
            VirtualMachineCpuIdInfoSpec[] machineCpuIdInfoSpecArray = new VirtualMachineCpuIdInfoSpec[srcCpuFeatureMask.Length];
            for (int index = 0; index < srcCpuFeatureMask.Length; ++index)
            {
                machineCpuIdInfoSpecArray[index] = new VirtualMachineCpuIdInfoSpec();
                machineCpuIdInfoSpecArray[index].info = srcCpuFeatureMask[index];
            }
            return machineCpuIdInfoSpecArray;
        }

        private static VmConfigSpec VmConfigInfoToVmConfigSpec(VmConfigInfo vmConfigInfo)
        {
            if (vmConfigInfo == null)
                return null;
            VmConfigSpec vmConfigSpec = new VmConfigSpec();
            vmConfigSpec.eula = vmConfigInfo.eula;
            vmConfigSpec.installBootRequired = vmConfigInfo.installBootRequired;
            vmConfigSpec.installBootRequiredSpecified = true;
            vmConfigSpec.installBootStopDelay = vmConfigInfo.installBootStopDelay;
            vmConfigSpec.installBootStopDelaySpecified = true;
            vmConfigSpec.ipAssignment = vmConfigInfo.ipAssignment;
            vmConfigSpec.ovfEnvironmentTransport = vmConfigInfo.ovfEnvironmentTransport;
            if (vmConfigInfo.ovfSection != null)
            {
                VAppOvfSectionSpec[] vappOvfSectionSpecArray = new VAppOvfSectionSpec[vmConfigInfo.ovfSection.Length];
                for (int index = 0; index < vmConfigInfo.ovfSection.Length; ++index)
                    vappOvfSectionSpecArray[index].info = vmConfigInfo.ovfSection[index];
                vmConfigSpec.ovfSection = vappOvfSectionSpecArray;
            }
            if (vmConfigInfo.product != null)
            {
                VAppProductSpec[] vappProductSpecArray = new VAppProductSpec[vmConfigInfo.product.Length];
                for (int index = 0; index < vmConfigInfo.product.Length; ++index)
                {
                    if (vmConfigInfo.product[index].instanceId != null)
                        vappProductSpecArray[index].info = vmConfigInfo.product[index];
                }
                vmConfigSpec.product = vappProductSpecArray;
            }
            if (vmConfigInfo.property != null)
            {
                VAppPropertySpec[] vappPropertySpecArray = new VAppPropertySpec[vmConfigInfo.property.Length];
                for (int index = 0; index < vmConfigInfo.property.Length; ++index)
                    vappPropertySpecArray[index].info = vmConfigInfo.property[index];
                vmConfigSpec.property = vappPropertySpecArray;
            }
            return vmConfigSpec;
        }

        private static string GetBracketedName(string name)
        {
            return "[" + name + "]";
        }
    }
}
