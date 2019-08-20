using System;

namespace Velo.Benchmark
{
    public static class Builder
    {
        public static BigObject CreateBigObject(Random random)
        {
            var seed = random.Next(0, 1000);
            return new BigObject
            {
                Array = new[] {seed + 1, seed + 2, seed + 3},
                Bool = seed % 10 == 0,
                Boo = new Boo
                {
                    Bool = seed % 5 == 0,
                    Double = seed + 1,
                    Float = seed + 2,
                    Int = seed + 3,
                },
                Foo = new Foo
                {
                    Bool = seed % 100 == 0,
                    Float = seed + 4,
                    Int = seed + 5,
                },
                Float = seed + 6,
                Int = seed + 7,
                Double = seed + 8,
                String = Guid.NewGuid().ToString()
            };
        }
    }
}