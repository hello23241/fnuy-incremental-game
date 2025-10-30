using BreakInfinity;

namespace WinFormsApp1.Core.Formatting;

public static class NumberFormatter
{
    public static string Format(BigDouble value)
    {
        if (value >= BigDouble.Pow(10, 308))
            return value.ToString("E1");

        string[] suffixes = {
            "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No",
            "Dc", "Ud", "Dd", "Td", "Qad", "Qid", "Sxd", "Spd", "Ocd", "Nod", "Vg",
            "Uvg", "Dvg", "Tvg", "Qavg", "Qivg", "Sxvg", "Spvg", "Ocvg", "Novg", "Tg",
            "Utg", "Dtg", "Ttg", "Qatg", "Qitg", "Sxtg", "Sptg", "Octg", "Notg", "Qag",
            "Uqag", "Dqag", "Tqag", "Qaqag", "Qiqag", "Sxqag", "Spqag", "Ocqag", "Noqag", "Qig",
            "Uqig", "Dqig", "Tqig", "Qaqig", "Qiqag", "Sxqig", "Spqig", "Ocqig", "Noqig", "Sxg",
            "Usxg", "Dsxg", "Tsxg", "Qasxg", "Qisxg", "Sxsxg", "Spsxg", "Ocsxg", "Nosxg", "Spg",
            "Uspg", "Dspg", "Tspg", "Qaspg", "Qispg", "Sxspg", "Spspg", "Ocspg", "Nospg", "Ocg",
            "Uocg", "Docg", "Tocg", "Qaocg", "Qiocg", "Sxocg", "Spocg", "Ococg", "Noocg", "Nog",
            "Unog", "Dnog", "Tnog", "Qanog", "Qinog", "Sxnog", "Spnog", "Ocnog", "Nonog", "C"
        };

        int suffixIndex = 0;
        while (value >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }
        return $"{value:F1}{suffixes[suffixIndex]}";
    }
}