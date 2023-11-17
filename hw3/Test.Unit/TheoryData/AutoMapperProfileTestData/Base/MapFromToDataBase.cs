using AutoFixture;
using AutoFixture.Kernel;

namespace Test.Unit.TheoryData.AutoMapperProfileTestData.Base;

public abstract class MapFromToDataBase<T, TR> : TheoryData<T, TR>
    where T : class where TR : class
{
    private const int TheoryDataCount = 3;

    protected MapFromToDataBase()
    {
        var autoFixture = new Fixture();
        autoFixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());

        for (var i = 0; i < TheoryDataCount; i++)
        {
            var from = autoFixture.Create<T>();

            // ReSharper disable once VirtualMemberCallInConstructor
            Add(from, MapByHand(from));
        }
    }

    protected abstract TR MapByHand(T product);

    private class UtcRandomDateTimeSequenceGenerator : ISpecimenBuilder
    {
        private readonly ISpecimenBuilder _innerRandomDateTimeSequenceGenerator;

        internal UtcRandomDateTimeSequenceGenerator()
        {
            _innerRandomDateTimeSequenceGenerator =
                new RandomDateTimeSequenceGenerator();
        }

        public object Create(object request, ISpecimenContext context)
        {
            var result =
                _innerRandomDateTimeSequenceGenerator.Create(request, context);
            if (result is NoSpecimen)
                return result;

            return ((DateTime)result).ToUniversalTime();
        }
    }
}