/*Thanks Dean from betking.IO for this code*/

using System;
using System.Security.Cryptography;
using Random = DiceBot.Sites.Random;

namespace DiceBot.Common
{
    public class MersenneTwister : Random
    {
#region "Destructor"

        ~MersenneTwister()
        {
            mt = null;
            mag01 = null;
        }

#endregion

#region "Get Unsigned Int 32bit number"

        /* generates a random number on [0,0xffffffff]-Interval */
        public uint genrand_Int32()
        {
            uint y;

            if (mti >= N)
            {
                /* generate N words at one time */
                short kk;

                if (mti == N + 1) /* if init_genrand() has not been called, */
                    init_genrand(5489); /* a default initial seed is used */

                for (kk = 0; kk < N - M; kk++)
                {
                    y = ((mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK)) >> 1;
                    mt[kk] = mt[kk + M] ^ mag01[mt[kk + 1] & 1] ^ y;
                }

                for (; kk < N - 1; kk++)
                {
                    y = ((mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK)) >> 1;
                    mt[kk] = mt[kk + (M - N)] ^ mag01[mt[kk + 1] & 1] ^ y;
                }

                y = ((mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK)) >> 1;
                mt[N - 1] = mt[M - 1] ^ mag01[mt[0] & 1] ^ y;

                mti = 0;
            }

            y = mt[mti++];

            /* Tempering */
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= y >> 18;

            return y;
        }

#endregion

#region "Get Int31 number"

        /* generates a random number on [0,0x7fffffff]-Interval */
        public uint genrand_Int31()
        {
            return genrand_Int32() >> 1;
        }

#endregion

        public override int Next(int max)
        {
            return (int) (genrand_Int32() % (uint) max);
        }

        public int Next(uint max)
        {
            return (int) (genrand_Int32() % max);
        }

        public override int Next()
        {
            return (int) genrand_Int32();
        }

        public override int Next(int minValue, int maxValue)
        {
            return minValue + Next(maxValue - minValue);
        }

#region "Private Parameter"

        /* Period parameters */
        private const short N = 624;
        private const short M = 397;
        private const uint MATRIX_A = 0x9908b0df; /* constant vector a */
        private const uint UPPER_MASK = 0x80000000; /* most significant w-r bits */
        private const uint LOWER_MASK = 0x7fffffff; /* least significant r bits */
        private uint[] mt; /* the array for the state vector  */
        private ushort mti; /* mti==N+1 means mt[N] is not initialized */
        private uint[] mag01;

#endregion

#region "Constructor"

        public MersenneTwister(uint s)
        {
            MT();
            init_genrand(s);
        }

        // coded by Mitil. 2006/01/04
        public MersenneTwister()
        {
            MT();

            // auto generate seed for .NET
            var seed_key = new uint[6];
            var rnseed = new byte[8];

            seed_key[0] = (uint) DateTime.Now.Millisecond;
            seed_key[1] = (uint) DateTime.Now.Second;
            seed_key[2] = (uint) DateTime.Now.DayOfYear;
            seed_key[3] = (uint) DateTime.Now.Year;

            RandomNumberGenerator rn
                = new RNGCryptoServiceProvider();

            rn.GetNonZeroBytes(rnseed);

            seed_key[4] = ((uint) rnseed[0] << 24) | ((uint) rnseed[1] << 16)
                                                   | ((uint) rnseed[2] << 8) | rnseed[3];

            seed_key[5] = ((uint) rnseed[4] << 24) | ((uint) rnseed[5] << 16)
                                                   | ((uint) rnseed[6] << 8) | rnseed[7];

            init_by_array(seed_key);

            rn = null;
            seed_key = null;
            rnseed = null;
        }

        public MersenneTwister(uint[] init_key)
        {
            MT();

            init_by_array(init_key);
        }

        private void MT()
        {
            mt = new uint[N];

            mag01 = new uint[] {0, MATRIX_A};
            /* mag01[x] = x * MATRIX_A  for x=0,1 */

            mti = N + 1;
        }

#endregion

#region "seed init"

        /* initializes mt[N] with a seed */
        private void init_genrand(uint s)
        {
            mt[0] = s;

            for (mti = 1; mti < N; mti++)
            {
                mt[mti] =
                    1812433253 * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti;

                /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                /* In the previous versions, MSBs of the seed affect   */
                /* only MSBs of the array mt[].                        */
                /* 2002/01/09 modified by Makoto Matsumoto             */
            }
        }

        /* initialize by an array with array-length */
        /* init_key is the array for initializing keys */
        /* key_length is its length */
        /* slight change for C++, 2004/2/26 */
        private void init_by_array(uint[] init_key)
        {
            uint i, j;
            int k;
            var key_length = init_key.Length;

            init_genrand(19650218);
            i = 1;
            j = 0;
            k = N > key_length ? N : key_length;

            for (; k > 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525))
                        + init_key[j] + j; /* non linear */

                i++;
                j++;

                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }

                if (j >= key_length) j = 0;
            }

            for (k = N - 1; k > 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941))
                        - i; /* non linear */

                i++;

                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
            }

            mt[0] = 0x80000000; /* MSB is 1; assuring non-zero initial array */
        }

#endregion

#region "Get type'decimal' number"

        /* generates a random number on [0,1]-real-Interval */
        public decimal genrand_real1()
        {
            return genrand_Int32() * (1.0m / 4294967295.0m);
            /* divided by 2^32-1 */
        }

        /* generates a random number on [0,1)-real-Interval */
        public decimal genrand_real2()
        {
            return genrand_Int32() * (1.0m / 4294967296.0m);
            /* divided by 2^32 */
        }

        public decimal genrand_real22()
        {
            return genrand_Int32() * (1 / 4294967296);
            /* divided by 2^32 */
        }

        /* generates a random number on (0,1)-real-Interval */
        public decimal genrand_real3()
        {
            return (genrand_Int32() + 0.5m) * (1.0m / 4294967296.0m);
            /* divided by 2^32 */
        }

        /* generates a random number on [0,1) with 53-bit resolution*/
        public decimal genrand_res53()
        {
            uint a = genrand_Int32() >> 5, b = genrand_Int32() >> 6;

            return (a * 67108864.0m + b) * (1.0m / 9007199254740992.0m);
        }

        /* These real versions are due to Isaku Wada, 2002/01/09 added */

#endregion
    }
}
