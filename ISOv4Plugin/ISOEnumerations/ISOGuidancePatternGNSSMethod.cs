/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

namespace AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations
{
    public enum ISOGuidancePatternGNSSMethod
    {
        GNSSFix = 1,
        DGNSSFix = 2,
        PreciseGNSS = 3,
        RTKFixedInteger = 4,
        RTKFloat = 5,
        EstDRMode = 6,
        ManualInput = 7,
        SimulateMode = 8,
        DesktopGeneratedData = 16,
        Other = 17
    }
}
