namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IEnumeratedMeterFactory
    {
        IEnumeratedMeterCreator GetMeterCreator(int ddi);
    }

    public class EnumeratedMeterFactory : IEnumeratedMeterFactory
    {
        public IEnumeratedMeterCreator GetMeterCreator(int ddi)
        {
            if (ddi == 141)
                return new WorkStateMeterCreator(ddi);
            if (ddi == 157)
                return new ConnectorTypeMeterCreator(ddi);
            if (ddi == 158)
                return new PrescriptionControlMeterCreator(ddi);
            if (ddi == 160)
                return new SectionControlStateMeterCreator(ddi);
            if (ddi >= 161 && ddi <= 176)
                return new CondensedWorkStateMeterCreator(ddi, 161);
            if (ddi == 210)
                return new SkyConditionsMeterCreator(ddi);
            if (ddi == 230)
                return new NetWeightStateMeterCreator(ddi);
            if (ddi == 240)
                return new ActualLoadingSystemStatusMeterCreator(ddi);
            if (ddi >= 290 && ddi < 305)
                return new CondensedWorkStateMeterCreator(ddi, 290);
            if (ddi >= 367 && ddi <= 382)
                return new CondensedSectionOverrideStateMeterCreator(ddi);
            return null;
        }
    }
}
