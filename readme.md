# Infoveave Authorisation & API Server

&copy; 2015-2016, Noesys Software Pvt Ltd. 

Dual Licensed under Infoveave Commercial and APLv3

You should have received a copy of the GNU Affero General Public License v3
along with this program (Infoveave)
You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the Infoveave without
disclosing the source code of your own applications.

---
### Prerequesites

- .net Framework 4.6.1
- .net Core RTM (For OSS)
- Visual Studio 2015 UPdate 3 (Community / Professional)

---

## Development

## Authorisation and API Server

- Running the default project `Infoveave` will launch Server
- The default page `/docs/index.html` is API Explorer


### Migrations
- Migrations are created and managed in `Infoveave.Data` project
- New migration must follow the format of `v{major}_{minor}_{xx}` naming
- To create a migration go to `Infoveave.Data` folder
- Run `dotnet ef migrations add v2_{minor}_{migration}`


### Client
- Clone the Infoveave-WebClient repository seperately and copy the output to wwwroot folder