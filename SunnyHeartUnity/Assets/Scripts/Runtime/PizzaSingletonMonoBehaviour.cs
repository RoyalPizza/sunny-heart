namespace Pizza.Runtime
{
    public abstract class PizzaSingletonMonoBehaviour<T> : PizzaMonoBehaviour where T : PizzaMonoBehaviour
    {
        public static T shared { get; private set; }
        public virtual bool IsDontDestroy() => true;

        protected virtual void Awake()
        {
            if (shared == null)
            {
                shared = this as T;

                if (IsDontDestroy())
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}