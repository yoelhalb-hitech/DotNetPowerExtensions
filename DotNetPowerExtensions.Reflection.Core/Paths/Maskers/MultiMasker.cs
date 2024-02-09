namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths.Maskers;

internal class MultiMasker : IMasker
{
    private readonly IMasker[] maskers;

    public MultiMasker(IMasker[] maskers)
    {
        this.maskers = maskers;
    }

    public string Mask(string str)
    {
        var lastStr = str;
        do
        {
            lastStr = str;
            for (int i = 0; i < maskers.Length; i++) str = maskers[i].Mask(str);
        }
        while (lastStr != str); // We need to try it many times since after the later maskings maybe there is another first to mask        

        return str;
    }
    public string Unmask(string str) => Unmask(new[] { str }).First();
    public IEnumerable<string> Unmask(IEnumerable<string> strs)
    {
        var strsArray = strs.ToArray();
        var lastStrs = strsArray;
        do
        {
            lastStrs = strsArray;
            // Unamsk in reverse order
            for (int i = maskers.Length - 1; i >= 0; i--) strsArray = maskers[i].Unmask(strsArray).ToArray();
        }
        while (Enumerable.Range(0, strsArray.Length).Any(i => strsArray[i] != lastStrs[i])); // We need to try it many times since after the later maskings maybe there is another first to mask        

        return strsArray;
    }
}
