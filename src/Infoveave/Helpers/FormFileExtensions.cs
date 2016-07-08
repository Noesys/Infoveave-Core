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
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CS1591
public static class FormFileExtensions
{
    private static int DefaultBufferSize = 80 * 1024;
    /// <summary>
    /// Asynchronously saves the contents of an uploaded file.
    /// </summary>
    /// <param name="formFile">The <see cref="IFormFile"/>.</param>
    /// <param name="filename">The name of the file to create.</param>
    /// <param name="cancellationToken"></param>
    public async static Task SaveAsAsync(
        this IFormFile formFile,
        string filename,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (formFile == null)
        {
            throw new ArgumentNullException(nameof(formFile));
        }

        using (var fileStream = new FileStream(filename, FileMode.Create))
        {
            var inputStream = formFile.OpenReadStream();
            await inputStream.CopyToAsync(fileStream, DefaultBufferSize, cancellationToken);
        }
    }
}
#pragma warning restore CS1591