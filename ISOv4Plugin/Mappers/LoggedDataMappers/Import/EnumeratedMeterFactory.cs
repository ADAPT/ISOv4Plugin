namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IEnumeratedMeterFactory
    {
        IEnumeratedMeterCreator GetMeterCreator(int? ddi);
    }

    public class EnumeratedMeterFactory : IEnumeratedMeterFactory
    {
        public IEnumeratedMeterCreator GetMeterCreator(int? ddi)
        {
            if (ddi == null)
                return null;
            
            if (ddi == 141)
                return new WorkStateMeterCreator((int)ddi);
            if (ddi == 157)
                return new ConnectorTypeMeterCreator((int)ddi);
            if (ddi == 158)
                return new PrescriptionControlMeterCreator((int)ddi);
            if (ddi == 160)
                return new SectionControlStateMeterCreator((int)ddi);
            if (ddi >= 161 && ddi <= 176)
                return new CondensedWorkStateMeterCreator((int)ddi, 161);
            if (ddi == 210)
                return new SkyConditionsMeterCreator((int)ddi);
            if (ddi == 230)
                return new NetWeightStateMeterCreator((int)ddi);
            if (ddi == 240)
                return new ActualLoadingSystemStatusMeterCreator((int)ddi);
            if (ddi >= 290 && ddi < 305)
                return new CondensedWorkStateMeterCreator((int)ddi, 290);
            if (ddi >= 367 && ddi <= 382)
                return new CondensedSectionOverrideStateMeterCreator((int)ddi);
            return null;
        }

        public static bool IsCondensedMeter(int? ddi)
        {
            return (ddi >= 161 && ddi <= 176) || (ddi >= 290 && ddi < 305) || (ddi >= 367 && ddi <= 382);
        }
    }
}
