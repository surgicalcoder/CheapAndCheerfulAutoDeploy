using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheapAndCheerfulAutoDeploy.Json.Appsettings;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CheapAndCheerfulAutoDeploy.Controllers
{
    [ApiController]
    public class DController : ControllerBase
    {
        private Appsetting appSetting;

        public DController(Appsetting appSetting)
        {
            this.appSetting = appSetting;
        }

        [Route("/d/{key}/{name}/{tag}")]
        public async Task<ActionResult> Deploy(string key, string name, string tag)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(tag))
            {
                return NotFound();
            }

            if (key != appSetting.Key)
            {
                return NotFound();
            }

            var deets = appSetting.Containers.FirstOrDefault(e => e.Name == name);

            if (deets == null)
            {
                return NotFound();
            }

            using var client = new DockerClientConfiguration().CreateClient();
            
            var progress = new Progress<JSONMessage>();

            var authConfig = new AuthConfig();

            if (appSetting.RepositoryAuth != null)
            {
                if (!string.IsNullOrWhiteSpace(appSetting.RepositoryAuth.Email))
                {
                    authConfig.Email = appSetting.RepositoryAuth.Email;
                }

                if (!string.IsNullOrWhiteSpace(appSetting.RepositoryAuth.Username))
                {
                    authConfig.Username = appSetting.RepositoryAuth.Username;
                }

                if (!string.IsNullOrWhiteSpace(appSetting.RepositoryAuth.Password))
                {
                    authConfig.Password = appSetting.RepositoryAuth.Password;
                }
            }

            var item = await client.Containers.InspectContainerAsync(name);

            await client.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = deets.Repository, Tag = tag }, authConfig, progress, CancellationToken.None);
            
            await client.Containers.RemoveContainerAsync(item.ID, new ContainerRemoveParameters()
            {
                Force = true, RemoveVolumes = true
            }, CancellationToken.None);

            var createContainerParameters = new CreateContainerParameters(item.Config)
            {
                Name = name, 
                Image = $"{deets.Repository}:{tag}",
                HostConfig = item.HostConfig
            };
            
            await client.Containers.CreateContainerAsync(createContainerParameters, CancellationToken.None);

            await client.Containers.StartContainerAsync(name, new ContainerStartParameters() { }, CancellationToken.None);

            return Ok();
        }

        /*
        [Route("/d/{key}/{action}/{tag}/{*items}")]
        public async Task<ActionResult> Pull(string key, string items, string tag)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return NotFound();
            }

            if (key != appSetting.Key)
            {
                return NotFound();
            }

            using (var client = new DockerClientConfiguration(new Uri(appSetting.Pipe)).CreateClient())
            {
                var progress = new Progress<JSONMessage>();
                await client.Images.CreateImageAsync(new ImagesCreateParameters()
                    {
                        FromImage = items,
                        Tag = tag
                    },
                    new AuthConfig()
                    {
                        Email = appSetting.RepositoryAuth.Username,
                        Username = appSetting.RepositoryAuth.Username,
                        Password = appSetting.RepositoryAuth.Password,
                        
                    }, progress, CancellationToken.None);

                return Ok();
            }
        }
        
        
        [Route("/d/{key}/{action}/{container}/{tag}/{*items}")]
        public async Task<ActionResult> Deploy(string key, string container, string items, string tag)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return NotFound();
            }

            if (key != appSetting.Key)
            {
                return NotFound();
            }

            if (!appSetting.Containers.Contains(container))
            {
                return NotFound();
            }

            using (var client = new DockerClientConfiguration(new Uri(appSetting.Pipe)).CreateClient())
            {
                var item = await client.Containers.InspectContainerAsync(container);
                await client.Containers.StopContainerAsync(item.ID, new ContainerStopParameters(){WaitBeforeKillSeconds = 1}, CancellationToken.None);
                await client.Containers.RemoveContainerAsync(item.ID, new ContainerRemoveParameters(), CancellationToken.None);
                await client.Containers.CreateContainerAsync(new CreateContainerParameters(item.Config)
                {
                    Name = container,
                    Image = $"{items}:{tag}",
                }, CancellationToken.None);
                await client.Containers.StartContainerAsync(container, new ContainerStartParameters() { }, CancellationToken.None);
                return Ok();
            }
        }*/
    }
}