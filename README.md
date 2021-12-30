# CheapAndCheerfulAutoDeploy
A cheap and cheerful auto deploy system for Windows and Docker. Will update existing containers based on web requests it recieves, locked down to a specific container repository.

This project gets automatically built and deployed onto [Docker Hub](https://hub.docker.com/r/surgicalcoder/cheapandcheerfulautodeploy) as it gets updated.

# How to use

Run the container on your server, mapping files and folders like so:

```docker run -d -v \\.\pipe\docker_engine:\\.\pipe\docker_engine -v C:\Apps\https_cert:c:\https --env-file .\envfile --label-file .\labelfile surgicalcoder/cheapandcheerfulautodeploy:ef72b62a8fbb43d4166c1ea27bd05c46751f1984```

You might need a HTTPS cert, for HTTPS'ing. In the environment file, set up a number of settings like:

```
Key=abcdef12345
RepositoryAuth:Email=yourlogingoeshere
RepositoryAuth:Username=yourlogingoeshere
RepositoryAuth:Password=MyUltraTopSecretPassword
Containers:0:Name=container_1_name
Containers:0:Repository=repositorylocation/location2
```

Then use the Key in the URL when it gets hit, format for this is:

```
https://full.url.to.instance/d/{key}/{docker name}/$(tag)
```

May take a bit of time depending on how fast network, disk, IO etc is.

# Questions

## Why on earth do I need this?

Well, you probably don't. This was built as a quick CICD type project, to enable containers to be updated on my own infrastructure for dev purposes.

## But you can use MiniKube/Kubenetes/Azure/Amazon/GCP/Whatever instead of this, why not use that?

If you want to, sure, go for it. That's not the use case for this.

## Your code is horrible! There's nothing I hate more in life than this project!

Thank you. That means alot to me.

