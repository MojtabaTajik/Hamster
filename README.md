# **Hamster**<h1>
### **Cross-platform agent to backup directories to AmazonS3 object storage** <h6>

![Hamster Logo](/resources/hamster.jpg)

We need a free and reliable solution to backup our sensitive data to a safe place.

After researching different solutions and finding free ones, they all depend on our servers and infrastructure. Still, we want to store all data in other cloud servers for fault tolerance and reliability.

We decided to develop a tool to run on all of our servers to backup databases/directories and upload them to the Arvan object storage infrastructure based on AmazonS3, so the solution is compatible with all AmazonS3 services.

I called it hamsters because Hamsters don't just stuff their faces with food -- they can also pack up their cheek pouches with babies. Mother hamsters are protective of their babies, and when they perceive a threat to the newborn clan, she may stuff the babies into her mouth and hide the babies in her cheek ;)

So Hamster was born. She protects all our services from evil!

Hamster needs a config file inside the executable, which contains the S3 secrets and backup configurations:

```
{
    "EndpointURL": "https://s3.ir-thr-at1.arvanstorage.com",
    "AccessKey": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "SecretKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Operations":
    [
        {
            "Name": "Test",
            "Command": "echo Hiiii > $Name/Test.txt",
            "RemoteFileName": "$Name-$Date.zip"
        },
        {
            "Name": "UserServer-DB",
            "Command": "docker exec \"DB-SqlServer\" /opt/mssql-tools/bin/sqlcmd -b -V16 -S localhost -U SA -P Nw$4*deio@#IYEU2we!e3EW -Q \"BACKUP DATABASE [UserDB] TO DISK = N'/var/opt/mssql/backups/Backup.bak' with NOFORMAT, INIT, NAME = 'UserDB-full', SKIP\" && docker cp DB-SqlServer:/var/opt/mssql/backups/Backup.bak $Name/Backup.bak",
            "RemoteFileName": "$Name-$Date.zip",
            "PersianDate": false
        },
        {
            "Name": "StreamServer-Media",
            "Command": "tar -czf $Name/StreamServer-Media.zip /media/StreamStorage",
            "RemoteFileName": "$Name-$Date.zip",
            "PersianDate": true
        },
        {
            "Name": "StreamServer-DB",
            "Command": "cid=$(docker ps | grep \"StreamDB\" | awk '{ print $1 }') && docker exec \"$cid\" /opt/mssql-tools/bin/sqlcmd -b -V16 -S localhost -U SA -P 'sHF4aS5apVVGZaP4CB7b87ew5WCq6MjsqptqMvC5yyTkxbAkj' -Q \"BACKUP DATABASE [cafedb] TO DISK = N'/var/opt/mssql/backups/Backup.bak' with NOFORMAT, INIT, NAME = 'Ravitoon-full', SKIP\" && docker cp $cid:/var/opt/mssql/backups/Backup.bak $Name/Backup.bak",
            "RemoteFileName": "$Name-$Date.zip",
            "PersianDate": true
        }
}
```

The $Name and $Date is predefined variables which replaces at runtime with actual values, you can use them in Command or RemoteFileName fields of operation.

> $Date => Replaced with current date time : 2022.15.07-15.53.16
> 
> $Name => Replaced with operation name defined in Name field    


The backup operation starts by passing the operation name defined in the config file to the Hamster executable:

```
sudo chmod +x hmaster
hamster UserServer-DB
```

Based on our needs, Hamster currently supports single operation execution, but this behaviour could change easily in code.

We use Jenkins in our CI/CD pipeline and use it to schedule the backup operations on multiple servers; this way, we don't need to copy and configure each server manually; we define a freestyle project for each server and define the schedule of backup operation using CRON time and execute Hamster with the proper command line on the target server.