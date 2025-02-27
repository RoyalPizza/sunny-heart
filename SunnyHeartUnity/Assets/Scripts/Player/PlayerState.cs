using Pizza.Runtime;

namespace Pizza
{
    public enum PlayerStateEnum { Idle, Walk, Jump, Fall, Dash }

    public class PlayerState : PizzaSingletonMonoBehaviour<PlayerState>
    {
        public override bool IsDontDestroy() => false;

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public PlayerStateEnum State { get; set; }

        public void SetDefaultValues()
        {
            Health = 3;
            MaxHealth = 3;
            State = PlayerStateEnum.Idle;
        }
    }
}
