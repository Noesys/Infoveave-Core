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

namespace Infoveave.Helpers
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ShortCodeHelper
    {
        public static string GetShortCode(int length)
        {
            const string legalCharacters = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var randomText = string.Empty;
            var rnd = new Random();
            for (int i = 0; i <= length; i++)
            {
                int iRandom = rnd.Next(0, legalCharacters.Length - 1);
                randomText += legalCharacters.Substring(iRandom, 1);
            }
            return randomText;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}