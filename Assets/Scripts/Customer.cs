using UnityEngine;

namespace Orders
{
    public class Customer : MonoBehaviour
    {
        private Animator animator;
        private Order order;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        public Order GetOrder()
        {
            return order;
        }

        public void PlaceOrder()
        {
            order = new Order();
        }

        public void Leave()
        {
            animator.SetTrigger("Leave");
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}
