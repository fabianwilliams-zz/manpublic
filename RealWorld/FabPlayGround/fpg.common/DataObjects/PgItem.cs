using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;

namespace fpg.common.DataObjects
{

        public class PgItem 
        {
            public string PlaygroundName { get; set; }
            public string PlaygroundMainImage { get; set; }
            public string PlaygroundMainImageThumb { get; set; }
            public PlaygroundAddress PlaygroundAddress { get; set; }
            public PlaygroundGeoLonLat PlaygroundGeoLonLat { get; set; }
            public List<Amenity> Amenities { get; set; }
            public Added Added { get; set; }
            public List<Visit> Visit { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }

        public class PlaygroundAddress
        {
            public string StreetAddress { get; set; }
            public string StreetAddress2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string Region { get; set; }
            public string Country { get; set; }
            public string Phone { get; set; }
        }

        public class PlaygroundGeoLonLat
        {
            public string Lon { get; set; }
            public string Lat { get; set; }
        }

        public class Picture
        {
            public string Image { get; set; }
        }

        public class Amenity
        {
            public string Name { get; set; }
            public string Amount { get; set; }
            public List<Picture> Pictures { get; set; }
        }

        public class Origin
        {
            public string Idp { get; set; }
            public string UUid { get; set; }
            public string Email { get; set; }
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Origin> Origin { get; set; }
        }

        public class Added
        {
            public string On { get; set; }
            public Person Person { get; set; }
        }

        public class Origin2
        {
            public string Idp { get; set; }
            public string UUid { get; set; }
            public string Email { get; set; }
        }

        public class Person2
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Origin2> Origin { get; set; }
        }

        public class Visit
        {
            public string On { get; set; }
            public string Ratings { get; set; }
            public string Comments { get; set; }
            public Person2 Person { get; set; }
        }
        /*
        public class RootObject
        {
            public string PlaygroundName { get; set; }
            public PlaygroundAddress PlaygroundAddress { get; set; }
            public PlaygroundGeoLonLat PlaygroundGeoLonLat { get; set; }
            public List<Amenity> Amenities { get; set; }
            public Added Added { get; set; }
            public List<Visit> Visit { get; set; }
            public string id { get; set; }
        }
        */
}
