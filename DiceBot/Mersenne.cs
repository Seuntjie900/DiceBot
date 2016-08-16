/*Thanks Dean from betking.IO for this code*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiceBot
{
    public class MersenneTwister : Random
    {

        #region "Private Parameter"
        /* Period parameters */
        private const Int16 N = 624;
        private const Int16 M = 397;
        private const UInt32 MATRIX_A = (UInt32)0x9908b0df;   /* constant vector a */
        private const UInt32 UPPER_MASK = (UInt32)0x80000000; /* most significant w-r bits */
        private const UInt32 LOWER_MASK = (UInt32)0x7fffffff; /* least significant r bits */
        private UInt32[] mt; /* the array for the state vector  */
        private UInt16 mti; /* mti==N+1 means mt[N] is not initialized */
        private UInt32[] mag01;
        #endregion

        #region "Constructor"

        public MersenneTwister(UInt32 s)
        {
            MT();
            init_genrand(s);
        }

        // coded by Mitil. 2006/01/04
        public MersenneTwister()
        {
            MT();

            // auto generate seed for .NET
            UInt32[] seed_key = new UInt32[6];
            Byte[] rnseed = new Byte[8];

            seed_key[0] = (UInt32)DateTime.Now.Millisecond;
            seed_key[1] = (UInt32)DateTime.Now.Second;
            seed_key[2] = (UInt32)DateTime.Now.DayOfYear;
            seed_key[3] = (UInt32)DateTime.Now.Year;
            RandomNumberGenerator rn
                = new RNGCryptoServiceProvider();
            rn.GetNonZeroBytes(rnseed);

            seed_key[4] = ((UInt32)rnseed[0] << 24) | ((UInt32)rnseed[1] << 16)
                          | ((UInt32)rnseed[2] << 8) | (UInt32)rnseed[3];
            seed_key[5] = ((UInt32)rnseed[4] << 24) | ((UInt32)rnseed[5] << 16)
                          | ((UInt32)rnseed[6] << 8) | (UInt32)rnseed[7];

            init_by_array(seed_key);

            rn = null;
            seed_key = null;
            rnseed = null;
        }

        public MersenneTwister(UInt32[] init_key)
        {
            MT();

            init_by_array(init_key);
        }

        private void MT()
        {
            mt = new UInt32[N];

            mag01 = new UInt32[] { 0, MATRIX_A };
            /* mag01[x] = x * MATRIX_A  for x=0,1 */

            mti = N + 1;
        }

        #endregion

        #region "Destructor"
        ~MersenneTwister()
        {
            mt = null;
            mag01 = null;
        }
        #endregion

        #region "seed init"
        /* initializes mt[N] with a seed */
        private void init_genrand(UInt32 s)
        {
            mt[0] = s;

            for (mti = 1; mti < N; mti++)
            {
                mt[mti] =
                    (UInt32)1812433253 * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti;
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
        private void init_by_array(UInt32[] init_key)
        {
            UInt32 i, j;
            Int32 k;
            Int32 key_length = init_key.Length;

            init_genrand(19650218);
            i = 1; j = 0;
            k = N > key_length ? N : key_length;

            for (; k > 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * (UInt32)1664525))
                        + init_key[j] + (UInt32)j; /* non linear */
                i++; j++;
                if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                if (j >= key_length) j = 0;
            }
            for (k = N - 1; k > 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * (UInt32)1566083941))
                        - (UInt32)i; /* non linear */
                i++;
                if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
            }

            mt[0] = 0x80000000; /* MSB is 1; assuring non-zero initial array */
        }
        #endregion

        #region "Get Unsigned Int 32bit number"
        /* generates a random number on [0,0xffffffff]-Interval */
        public UInt32 genrand_Int32()
        {
            UInt32 y;

            if (mti >= N)
            { /* generate N words at one time */
                Int16 kk;

                if (mti == N + 1)   /* if init_genrand() has not been called, */
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
        public UInt32 genrand_Int31()
        {
            return genrand_Int32() >> 1;
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
            UInt32 a = genrand_Int32() >> 5, b = genrand_Int32() >> 6;
            return (a * 67108864.0m + b) * (1.0m / 9007199254740992.0m);
        }
        /* These real versions are due to Isaku Wada, 2002/01/09 added */
        #endregion

        public override int Next(int max)
        {
            return (int)(genrand_Int32() % (uint)max);
        }

        public int Next(uint max)
        {
            return (int)(genrand_Int32() % max);
        }

        public override int Next()
        {
            return (int)genrand_Int32();
        }

        public override int Next(int minValue, int maxValue)
        {
            return minValue + Next(maxValue - minValue);
        }
    }
}




