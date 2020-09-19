using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqRanking
{
    public class UniqPoint
    {
        public UniqPoint(int id ,string uniqName ,int point , int uniqID)
        {
            this.Id = id;
            this.UniqName = uniqName;
            this.Point = point;
            this.UniqID = uniqID;
        }
        public int Id { get; set; }
        public int UniqID { get; set; }
        public string UniqName { get; set; }
        public int Point { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", UniqName, Point);
        }
    }
}
