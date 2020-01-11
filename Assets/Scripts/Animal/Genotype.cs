using System;

namespace DefaultNamespace
{
    public class Genotype
    {
        public static int genotypeLength = 32;

        public Genotype(string sequence)
        {
            Sequence = sequence;
        }

        public Genotype(Int32 geneticNumber)
        {
            Sequence = Convert.ToString(geneticNumber, 2).PadLeft(genotypeLength, '0');
        }

        public Genotype()
        {
        }

        public string Sequence { get; set; }
    }
}