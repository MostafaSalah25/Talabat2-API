﻿using System;

namespace Talabat.DAL.Entities.Order_Aggregate
{
    public class Address 
    {
        public Address()
        {
        }
        public Address(string firstName, string lastName, string country, string city, string street)
        {
            FirstName = firstName;
            LastName = lastName;
            Country = country;
            City = city;
            Street = street;
        }
        public string FirstName { get; set; }  
        public string LastName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
    }
}
