using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomerAPI
{
   
        public sealed class CustomerManager
        {
            private static readonly CustomerManager _instance = null;

            private static readonly List<Customer> _customers;
            public static CustomerManager Instance
            {
            get { 
                
                    return _instance;
                }
            }

            static CustomerManager()
            {
                _instance = new CustomerManager();
            _customers = new List<Customer>();

            PopulateCustomers();
            }

        private static void PopulateCustomers()
        {
            _customers.Add(new Customer() { CustomerId = Guid.NewGuid(), CustomerName = "Globomantics", Email = "info@globomantics.com", Telephone = "221 232 332 211" });
            _customers.Add(new Customer() { CustomerId = Guid.NewGuid(), CustomerName = "Wirwed Brain Coffee", Email = "info@wiredbraincoffee.com", Telephone = "543 284 294 573" });

        }

        private CustomerManager()
            {
            }

            public void AddBook(Customer customer)
            {
                if (customer != null)
                {
                    _customers.Add(customer);
                }
            }

            public void UpdateCustomer(Customer customer)
            {
                var customerToUpdate = _customers.SingleOrDefault(b => b.CustomerId.Equals(customer.CustomerId));

                if (customerToUpdate != null)
                {
                customerToUpdate.CustomerName = customer.CustomerName;
                customerToUpdate.Telephone = customer.Telephone;
                customerToUpdate.Email = customer.Email;
                }
            }

            public IEnumerable<Customer> GetCustomers()
            {
                return _customers.ToArray();
            }

            public bool DeleteCustomer(string id)
            {
                bool deleted = false;
                Guid guidToRemove = Guid.Parse(id);
                var customerToRemove = _customers.SingleOrDefault(b => b.CustomerId.Equals(guidToRemove));
            if (customerToRemove != null)
                {
                    _customers.Remove(customerToRemove);
                    deleted = true;
                }

                return deleted;
            }

            public Customer GetCustomerById(string id)
            {
                Guid guid = Guid.Parse(id);
                return _customers.SingleOrDefault(b => b.CustomerId.Equals(guid));
            }
        }
    }


