using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace DefaultNamespace
{
    public class Genetics
    {
        private static int genotypeLength = Genotype.genotypeLength;
        public static float mutationProbability = 0.05f;
        public static int crossoverSiteNumber = 8;
        public static int maxNumberOfMutations = 4;

        // public static int maxNumberOfSites = (genotypeLength / 2);
        //public static RangeInt crossoverSiteRange = new RangeInt(1, genotypeLength);

        private static Random r = new Random();

        public static Genotype Crossover(Genotype a, Genotype b)
        {
            Genotype offspring = new Genotype();
            int crossoverPoint = r.Next(1, genotypeLength);

            if (r.NextDouble() > 0.5f)
            {
                string newSequence = a.Sequence.Substring(0, crossoverPoint) + b.Sequence.Substring(crossoverPoint);
                offspring.Sequence = newSequence;
            }
            else
            {
                string newSequence = b.Sequence.Substring(0, crossoverPoint) + a.Sequence.Substring(crossoverPoint);
                offspring.Sequence = newSequence;
            }

            return offspring;
        }

        public static Genotype MultipleCrossover(Genotype a, Genotype b)
        {
            StringBuilder sb = new StringBuilder();
            List<int> crossoverSites = GetSortedRandomUniqueNumbers(1, genotypeLength, crossoverSiteNumber);

            sb.Append(ChooseParent(a, b).Sequence.Substring(0, crossoverSites[0]));

            for (int i = 0; i < crossoverSites.Count - 1; i++)
            {
                Genotype parent = ChooseParent(a, b);
                int startIndex = crossoverSites[i];
                int substringLength = crossoverSites[i + 1] - startIndex;
                sb.Append(parent.Sequence.Substring(startIndex, substringLength));
            }

            sb.Append(ChooseParent(a, b).Sequence.Substring(crossoverSites[crossoverSites.Count - 1]));

            Genotype offspring = new Genotype(sb.ToString());
            return offspring;
        }

        private static Genotype ChooseParent(Genotype a, Genotype b)
        {
            if (r.NextDouble() > 0.5f)
                return a;
            return b;
        }

        // public static Genotype MultipleRandomCrossover(Genotype a, Genotype b)
        // {
        //     int numberOfCrossoverSites = r.Next(1, maxNumberOfSites + 1); //Random.Range(1, maxNumberOfSites));
        //     List<int> crossoverSites = new List<int>(numberOfCrossoverSites);
        //     return a;
        // }

        public static Genotype Mutation(Genotype g)
        {
            List<int> possibleMutationSites = GetSortedRandomUniqueNumbers(1, genotypeLength, maxNumberOfMutations);
            StringBuilder sb = new StringBuilder(g.Sequence);
            foreach (var i in possibleMutationSites)
            {
                if (r.NextDouble() < mutationProbability)
                {
                    sb[i] = g.Sequence[i] != '0' ? '0' : '1';
                }
            }

            g.Sequence = sb.ToString();

            return g;
        }

        public static Dictionary<string, int> Decode(Genotype g)
        {
            // Traits

            // Fur color 0,8
            string furColorGene = g.Sequence.Substring(0, 8);
            // Speed 8,13
            string speedGene = g.Sequence.Substring(8, 5);
            // Fov Radius 15,21
            string fovRadiusGene = g.Sequence.Substring(15, 6);
            // Fov Angle 23,29
            string fovAngleGene = g.Sequence.Substring(23, 6);
            // Sex 31, 32
            string sexGene = g.Sequence.Substring(genotypeLength - 1, 1);


            Dictionary<string, int> geneDict = new Dictionary<string, int>();
            geneDict.Add("color", Convert.ToInt32(furColorGene, 2));
            geneDict.Add("speed", Convert.ToInt32(speedGene, 2));
            geneDict.Add("fovRadius", Convert.ToInt32(fovRadiusGene, 2));
            geneDict.Add("fovAngle", Convert.ToInt32(fovAngleGene, 2));
            geneDict.Add("sex", Convert.ToInt32(sexGene, 2));

            return geneDict;
        }

        private static List<int> GetSortedRandomUniqueNumbers(int start, int end, int count)
        {
            var uniqList = Enumerable.Range(start, end - start).OrderBy(g => Guid.NewGuid()).Take(count).ToList();
            uniqList.Sort();
            return uniqList;
        }
    }
}