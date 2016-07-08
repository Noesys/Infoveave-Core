/* Copyright © 2015-2016 Noesys Software Pvt.Ltd. - All Rights Reserved
 * -------------
 * This file is part of Infoveave.
 * Infoveave is dual licensed under Infoveave Commercial License and AGPL v3  
 * -------------
 * You should have received a copy of the GNU Affero General Public License v3
 * along with this program (Infoveave)
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the Infoveave without
 * disclosing the source code of your own applications.
 * -------------
 * Authors: Naresh Jois <naresh@noesyssoftware.com>, et al.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infoveave.AdapterFramework
{
    public class AnalysisQuerySerializable
    {
        public List<string> Measures { get; set; }
        public Dictionary<string, KeyValuePair<string,List<string>>> Dimensions { get; set; }
        public Dictionary<string, KeyValuePair<string,List<string>>> Filters { get; set; }
        public string CustomDateDimension { get; set; }
        public bool RetrieveEmpty { get; set; }
    }

    public class AnalysisQuery
    {
        private readonly string _cube;
        private List<string> _withMembers;
        private List<string> _measures;
        private Dictionary<string, KeyValuePair<string,List<string>>> _dimensions;
        private Dictionary<string, KeyValuePair<string, List<string>>> _filters;
        /// <summary>
        /// Create an Instance of an Analysis Query
        /// </summary>
        /// <param name="cube">Name of the Cube on which the query is formed</param>
        public AnalysisQuery(string cube)
        {
            if (string.IsNullOrWhiteSpace(cube)) throw new Exception("Cube Name Cannot be Empty");
            _cube = cube;
            _withMembers = new List<string>();
            _measures = new List<string>();
            _dimensions = new Dictionary<string, KeyValuePair<string,List<string>>>();
            _filters = new Dictionary<string, KeyValuePair<string, List<string>>>();
        }

        public object GetSerializable()
        {
            return new AnalysisQuerySerializable { Measures = _measures, Dimensions = _dimensions, Filters = _filters, CustomDateDimension = CustomDateDimension, RetrieveEmpty = RetrieveEmpty };
        }
        public void ApplyFromSerialized(AnalysisQuerySerializable data)
        {
            this._measures = data.Measures;
            this._dimensions = data.Dimensions;
            this._filters = data.Filters;
            this.CustomDateDimension = data.CustomDateDimension;
            this.RetrieveEmpty = data.RetrieveEmpty;
        }

        public List<String> WithMembers { get { return _withMembers; } set { _withMembers = value; } }

        public List<string> Measures { get { return _measures; } set { _measures = value; } }
        public Dictionary<string, KeyValuePair<string,List<string>>> Dimensions { get { return _dimensions; } set { _dimensions = value; } }
        public Dictionary<string, KeyValuePair<string, List<string>>> Filters { get { return _filters; } set { _filters = value; } }

        public string CustomDateDimension { get; set; }
        public bool RetrieveEmpty { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Cube { get { return _cube; } }
    }
}
