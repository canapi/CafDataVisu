using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;


namespace CafDataVisu.Controllers
{
    internal struct HierarchyRow
    {
        public int Nb { get; set; }
        public int NbSep { get; set; }
        //Complete Path
        public string PathMod { get; set; }
    }

    internal class HierarchyMember
    {
        //Count of allocataire starting with this path
        [JsonIgnore]
        public int AllocataireCount { get; set; }
        [JsonIgnore]
        public string PathFromStart { get; set; }
        [JsonIgnore]
        public string NodeName { get; set; }
        [JsonIgnore]
        public List<HierarchyMember> Children { get; set; }
        [JsonIgnore]
        public int Rate { get; set; }
        [JsonIgnore]
        public int Depth { get; set; }
        public string name {
            get{
                if(NodeName.Length>27)
                    return NodeName.Substring(0,26) + "..." + "("+ Rate.ToString()+"%)";
                return NodeName + "(" + Rate.ToString() + "%)";

            }
        }

        public bool free
        {
            get
            {
                return true;
            }
        }

        public string description
        {
            get
            {
                return NodeName + "(" + AllocataireCount.ToString() + ")";
            }
        }

        public HierarchyMember[] children
        {
            get
            {
                return Children.ToArray();
            }
        }

    }

    public class ParcoursAllocataireController : ApiController
    {
        string connectionString = "Server=10.20.7.19,1439;Database=Hackathon;User Id=scopit;Password=scopit;";
        List<HierarchyRow> rows = new List<HierarchyRow>();

        public string Get()
        {
            var res = GetTreeFromDb();

            return JsonConvert.SerializeObject(res);
        }

        private HierarchyMember GetTreeFromDb()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT [Nb] ,LEN([PathMod]) -LEN(REPLACE([PathMod], '|', '')) as nbSep   ,[PathMod]    FROM     [Hackathon].[dbo].[FAB2]      f";

                conn.Open();

                var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    rows.Add(
                        new HierarchyRow()
                        {
                            Nb =  reader.GetInt32(0),
                            NbSep = (int)reader.GetInt64(1),
                            PathMod = reader.GetString(2)
                        });
                }
                reader.Close();
                conn.Close();
            }

            HierarchyMember hierarchy = new HierarchyMember()
            {
                NodeName = "Tous les alocataires",
                PathFromStart = string.Empty,
                AllocataireCount = rows.Sum(c => c.Nb),
                Depth = 0
            };

            hierarchy.Children = GetHierarchyFromHierarchyRowList(hierarchy);

            return hierarchy;
        }

        private List<HierarchyMember> GetHierarchyFromHierarchyRowList(HierarchyMember parent)
        {
            List<HierarchyMember> res = new List<HierarchyMember>();

            List<string> children;

            //Select roots
            if (string.IsNullOrEmpty(parent.PathFromStart))
            {
                children = rows
                    .Where(r => r.PathMod.Split(new[] { '|' }).Length > 1)
                    .Select(r => r.PathMod.Substring(0, r.PathMod.IndexOf('|') )).Distinct().ToList();
            }
            else
            {
                children = rows
                    .Where(r => r.PathMod.StartsWith(parent.PathFromStart) && r.PathMod != parent.PathFromStart)
                    .Select(r => r.PathMod.Substring(parent.PathFromStart.Length + 1) )
                    .Select(path=>path.Contains("|") ? path.Substring(0, path.IndexOf('|')) : path)
                    .Where(path=>!string.IsNullOrEmpty(path))
                    .Distinct()
                    .ToList();
            }


            foreach (var child in children)
            {
                string parentPath = parent.PathFromStart + (string.IsNullOrEmpty(parent.PathFromStart) ? "" : "|");

                int allocCount = rows.Where(r => r.PathMod.StartsWith(parentPath + child)).Sum(r => r.Nb);

                HierarchyMember member = new HierarchyMember()
                {
                    AllocataireCount = allocCount,
                    NodeName = child,
                    PathFromStart = parentPath + child,
                    Rate = allocCount * 100 / parent.AllocataireCount,
                    Depth = parent.Depth+1
                };
                if (member.Depth <= 10)
                    member.Children = GetHierarchyFromHierarchyRowList(member);
                else
                    member.Children = new List<HierarchyMember>();

                res.Add(member);
            }


            return res;
        }
    }
}