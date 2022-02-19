using System;

namespace CustomerAPI
{
    public class Customer
    {
        public Guid CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }

    }
}
