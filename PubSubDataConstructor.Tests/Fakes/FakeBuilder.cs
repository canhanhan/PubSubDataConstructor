namespace PubSubDataConstructor.Tests.Fakes
{
    public class FakeBuilder : FluentBuilder<FakeEntity>
    {
        public FakeBuilder()
        {
            Map(x => x.Field1).NotBlank();
            Map(x => x.Field2).Min();
            Map(x => x.Field3).Min();
            Map(x => x.Field4).Min().NotBlank();
            Map(x => x.Field5).Max();
            Map(x => x.Field6).Max();
            Map(x => x.Field7).Join();
            Map(x => x.Field8).Union();
        }
    }
}
