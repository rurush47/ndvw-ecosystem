using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Mode Modifier/Attack Aligner")]
    public class MAttackAligner : ModeModifier
    {
        public FloatReference FindRadius = new FloatReference(2);

        public override void OnModeEnter(Mode mode)
        {
            MAnimal animal = mode.Animal;

            IAlign SelfAligner = animal.GetComponent<IAlign>();
            IAlign EnemyAligner = null;

            var pos = SelfAligner != null ? SelfAligner.MainPoint.position : animal.transform.position;

            var AllColliders = Physics.OverlapSphere(pos, FindRadius, animal.HitLayer);

            Collider MinDistanceCol;
            float Distance = float.MaxValue;

            foreach (var col in AllColliders)
            {
                if (col.GetComponentInParent<MAnimal>() == animal) continue; //Don't Find your own colliders

                var DistCol = Vector3.Distance(animal.transform.position, col.transform.position);

                if (Distance> DistCol)
                {
                    Distance = DistCol;
                    MinDistanceCol = col;
                    EnemyAligner = col.GetComponentInParent<IAlign>();
                }
            }

            if (EnemyAligner != null)
            {
                EnemyAligner.Align(animal.transform);
            }
        }
    }
}