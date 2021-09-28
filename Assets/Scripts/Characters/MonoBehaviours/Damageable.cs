using System;
using SceneManagement;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.MonoBehaviours
{
    public class Damageable : MonoBehaviour, IDataPersister
    {
        [Serializable]
        public class HealthEvent : UnityEvent<Damageable>
        { }
        
        [Serializable]
        public class DamageEvent : UnityEvent<Damager, Damageable>
        { }
        
        [Serializable]
        public class HealEvent : UnityEvent<int, Damageable>
        { }
        
        public int startingHealth = 5;
        public bool invulnerableAfterDamage = true;
        public float invulnerabilityDuration = 3f;
        public bool disableOnDeath;
        [Tooltip("An offset from the object position used to set from where the distance to the damager is computed")]
        public Vector2 centreOffset = new Vector2(0f, 1f);
        public HealthEvent onHealthSet;
        public DamageEvent onTakeDamage;
        public DamageEvent onDie;
        public HealEvent onGainHealth;
        [HideInInspector]
        public DataSettings dataSettings;

        private bool _invulnerable;
        private float _invulnerabilityTimer;
        private int _currentHealth;
        private Vector2 _damageDirection;
        private bool _resetHealthOnSceneReload;

        public int CurrentHealth
        {
            get { return _currentHealth; }
        }

        void OnEnable()
        {
            PersistentDataManager.RegisterPersister(this);
            _currentHealth = startingHealth;

            onHealthSet.Invoke(this);

            DisableInvulnerability();
        }

        void OnDisable()
        {
            PersistentDataManager.UnregisterPersister(this);
        }

        void Update()
        {
            if (_invulnerable)
            {
                _invulnerabilityTimer -= Time.deltaTime;

                if (_invulnerabilityTimer <= 0f)
                {
                    _invulnerable = false;
                }
            }
        }

        public void EnableInvulnerability(bool ignoreTimer = false)
        {
            _invulnerable = true;
            //technically don't ignore timer, just set it to an insanly big number. Allow to avoid to add more test & special case.
            _invulnerabilityTimer = ignoreTimer ? float.MaxValue : invulnerabilityDuration;
        }

        public void DisableInvulnerability()
        {
            _invulnerable = false;
        }

        public Vector2 GetDamageDirection()
        {
            return _damageDirection;
        }

        public void TakeDamage(Damager damager, bool ignoreInvincible = false)
        {
            if ((_invulnerable && !ignoreInvincible) || _currentHealth <= 0)
                return;

            //we can reach that point if the damager was one that was ignoring invincible state.
            //We still want the callback that we were hit, but not the damage to be removed from health.
            if (!_invulnerable)
            {
                _currentHealth -= damager.damage;
                onHealthSet.Invoke(this);
            }

            _damageDirection = transform.position + (Vector3)centreOffset - damager.transform.position;

            onTakeDamage.Invoke(damager, this);

            if (_currentHealth <= 0)
            {
                onDie.Invoke(damager, this);
                _resetHealthOnSceneReload = true;
                EnableInvulnerability();
                if (disableOnDeath) gameObject.SetActive(false);
            }
        }

        public void GainHealth(int amount)
        {
            _currentHealth += amount;

            if (_currentHealth > startingHealth)
                _currentHealth = startingHealth;

            onHealthSet.Invoke(this);

            onGainHealth.Invoke(amount, this);
        }

        public void SetHealth(int amount)
        {
            _currentHealth = amount;

            if (_currentHealth <= 0)
            {
                onDie.Invoke(null, this);
                _resetHealthOnSceneReload = true;
                EnableInvulnerability();
                if (disableOnDeath) gameObject.SetActive(false);
            }

            onHealthSet.Invoke(this);
        }

        public DataSettings GetDataSettings()
        {
            return dataSettings;
        }

        public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
        {
            dataSettings.dataTag = dataTag;
            dataSettings.persistenceType = persistenceType;
        }

        public Data SaveData()
        {
            return new Data<int, bool>(CurrentHealth, _resetHealthOnSceneReload);
        }

        public void LoadData(Data data)
        {
            Data<int, bool> healthData = (Data<int, bool>)data;
            _currentHealth = healthData.value1 ? startingHealth : healthData.value0;
            onHealthSet.Invoke(this);
        }
    }
}
