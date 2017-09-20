/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

namespace AgGateway.ADAPT.ISOv4Plugin.Representation
{
    public class DdiDefinition
    {
        public int Id { get; set; }
        public string Definition { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public double Resolution { get; set; }
    }
}