using Pizza.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Pizza
{
    public sealed class PlayerHUD : PizzaMonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Image[] _healthIcons;
        [SerializeField] private Color _healthFullColor = Color.white;
        [SerializeField] private Color _healthEmptyColor = Color.gray;

        private void Update()
        {
            UpdateHealthBar(PlayerState.shared.Health, PlayerState.shared.MaxHealth);
        }

        private void UpdateHealthBar(int health, int maxHealth)
        {
            for (int i = 0; i < _healthIcons.Length; i++)
            {
                // update color based on health
                if (i + 1 <= health)
                    _healthIcons[i].color = _healthFullColor;
                else
                    _healthIcons[i].color = _healthEmptyColor;

                // hide icons that are not unlocked
                if (i + 1 <= maxHealth && !_healthIcons[i].gameObject.activeInHierarchy)
                    _healthIcons[i].gameObject.SetActive(true);
                else if (i + 1 > maxHealth && _healthIcons[i].gameObject.activeInHierarchy)
                    _healthIcons[i].gameObject.SetActive(false);
            }
        }
    }

}
