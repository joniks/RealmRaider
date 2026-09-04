using RealmRaiders.Characters;
using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Traps
{
    public sealed class RootTrap : TrapBase
    {
        public bool RecentlyActivated => Time.time < feedbackUntil;
        float feedbackUntil;
        void Awake() { var collider = GetComponent<Collider>(); if (collider) collider.isTrigger = true; }
        public override bool TryActivate()
        {
            if (!base.TryActivate()) return false;
            feedbackUntil = Time.time + 1.1f;
            StartCoroutine(PulseAndMark());
            return true;
        }
        protected override void ActivateEffect(CombatEntity target)
        { target.ApplyRoot(2.25f); target.Health.TakeDamage(new DamageInfo(12, gameObject, target.transform.position), target.Stats.Armor); }

        System.Collections.IEnumerator PulseAndMark()
        {
            if (Visual) Visual.material.color = new Color(.85f, 1f, .18f);
            var marker = new GameObject("ROOTED!", typeof(TextMesh)); marker.transform.position = Target.transform.position + Vector3.up * 2.2f;
            var text = marker.GetComponent<TextMesh>(); text.text = "ROOTED!"; text.fontSize = 64; text.characterSize = .12f; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = new Color(.9f, 1f, .25f);
            var until = Time.time + 1.1f;
            while (Time.time < until)
            {
                var camera = Camera.main;
                if (camera) marker.transform.rotation = Quaternion.LookRotation(camera.transform.position - marker.transform.position, camera.transform.up);
                yield return null;
            }
            if (Visual) Visual.material.color = CooldownColor;
            Destroy(marker);
        }
    }
}
