using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace CheapAndCheerfulAutoDeploy.ConsoleCreator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new DockerClientConfiguration().CreateClient();

            foreach (var resp in await client.Containers.ListContainersAsync(new ContainersListParameters(){All = true}))
            {
                string name = resp.Names.FirstOrDefault()[1..];
                
                var deets = await client.Containers.InspectContainerAsync(resp.ID);

                await GenerateRunCommand(name, resp, deets);
                var imageDeets = await client.Images.InspectImageAsync(deets.Image);
                await GenerateEnvironmentFile(name, resp, deets, imageDeets);
                await GenerateLabelFile(name, resp, deets);
            }
        }

        private static async Task GenerateLabelFile(string name, ContainerListResponse resp, ContainerInspectResponse deets)
        {
            await File.WriteAllLinesAsync($"{name}.label", deets.Config.Labels.Select(e=>$"{e.Key}={e.Value}"));
        }

        private static async Task GenerateEnvironmentFile(string name, ContainerListResponse resp, ContainerInspectResponse deets, ImageInspectResponse imageDeets)
        {
            var containerEnv = deets.Config.Env;
            var imageEnv = imageDeets.ContainerConfig.Env;

            IEnumerable<string> leftOver;

            if (imageEnv != null && imageEnv.Any())
            {
                leftOver = containerEnv.Except(imageEnv);
            }
            else
            {
                leftOver = containerEnv;
            }
            
            await File.WriteAllLinesAsync($"{name}.env", leftOver);
        }

        private static async Task GenerateRunCommand(string name, ContainerListResponse resp, ContainerInspectResponse deets)
        {
            string template = $"docker run --name {name} -d {GenerateMountCommands(name, resp, deets)} --env-file .\\{name}.env --label-file .\\{name}.label {deets.Config.Image}";
            await File.WriteAllTextAsync($"{name}.bat", template);
        }

        private static string GenerateMountCommands(string name, ContainerListResponse resp, ContainerInspectResponse deets)
        {
            if (deets.HostConfig.Binds.Any())
            {
                return deets.HostConfig.Binds.Aggregate("", (current, hostConfigBind) => $"{current} -v {hostConfigBind}");
            }

            return string.Empty;
        }
    }
}