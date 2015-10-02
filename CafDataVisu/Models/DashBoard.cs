using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CafDataVisu.Models
{
    public class DashBoard
    {
        public string GroupName { get; set; }
        public string GroupId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Tile> Tiles { get; set; }
    }

    public class Tile
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}