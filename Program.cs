using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace docker_dotnet_test
{
    class Program
    {
        public static DockerClient client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        static void Main(string[] args)
        {
            Console.WriteLine("Docker.DotNet Demo!");

            PullImage().GetAwaiter().GetResult();
            CreateContainer().GetAwaiter().GetResult();
            ListContainers().GetAwaiter().GetResult();
            StartContainer().GetAwaiter().GetResult();
            StopContainer().GetAwaiter().GetResult();

            Console.WriteLine("Docker.DotNet Demo - END!");
        }
        static async Task PullImage() {
            var report = new Progress<JSONMessage>( msg =>
            {
                Console.WriteLine( $"{msg.Status}|{msg.ProgressMessage}|{msg.ErrorMessage}" );
            } );

            await client.Images.CreateImageAsync(new ImagesCreateParameters() {
                FromImage = "jwilder/whoami"
            }, null, report);
        }
        static async Task CreateContainer() {
            var container = await client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Name = "whoami",
                    Image = "jwilder/whoami",
                    // Env = new string[]{ "ACCEPT_EULA=y", "MSSQL_SA_PASSWORD=<YourStrong!Passw0rd" },
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                "8000/tcp",
                                new PortBinding[]
                                {
                                    new PortBinding
                                    {
                                        HostPort = "8000"
                                    }
                                }
                            }
                        }
                    }
                });

            Console.WriteLine("Name: " + container.ID);
        }

        static async Task ListContainers() {
            IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters(){
                    Limit = 10,
                });

            foreach (var container in containers) 
            {
                foreach (var name in container.Names) 
                {
                    Console.WriteLine(name);
                }
            }
        }

        static async Task StartContainer() {
            await client.Containers.StartContainerAsync("/whoami",  new ContainerStartParameters());
        }

        static async Task StopContainer() {
            var stopped = await client.Containers.StopContainerAsync("/whoami", new ContainerStopParameters(){
                    WaitBeforeKillSeconds = 30
                },
                CancellationToken.None);
            
            Console.WriteLine(stopped);
        }
    }
}
