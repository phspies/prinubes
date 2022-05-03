namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class SnapshotsSummary
    {
        public int NumOfSnapshots;
        internal Dictionary<DateTime, VMSDEntry> VmsdEntries;
        internal DateTime CreateTimeOfCurrent;
        public SnapshotNode Current;
        public SnapshotNode[] Roots;

        public bool IsConsistent()
        {
            int numOfSnapshots = this.NumOfSnapshots;
            int length = this.Roots.Length;
            foreach (SnapshotNode root in this.Roots)
                length += this.GetSnapshotNum(root);
            return numOfSnapshots == length;
        }

        private int GetSnapshotNum(SnapshotNode node)
        {
            int count = node.Children.Count;
            foreach (SnapshotNode child in node.Children)
                count += this.GetSnapshotNum(child);
            return count;
        }
    }
}
