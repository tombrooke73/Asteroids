#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Core;
using System.Linq;
using FTOptix.OPCUAServer;
using System.Collections.Generic;
#endregion

public class AsteroidGameLogic : BaseNetLogic
{
    #region Asteroids
    PeriodicTask bigAsteroid;
    PeriodicTask collisionLogic;
    PeriodicTask asteroidMovements;
    Image asteroid;

    Random r;
    List<Asteroid> runningBigAsteroids = new List<Asteroid>();
    List<Asteroid> waitingBigAsteroids = new List<Asteroid>();
    List<Asteroid> runningMediumAsteroids = new List<Asteroid>();
    List<Asteroid> waitingMediumAsteroids = new List<Asteroid>();
    List<Asteroid> runningMiniAsteroids = new List<Asteroid>();
    List<Asteroid> waitingMiniAsteroids = new List<Asteroid>();

    private void StartingBigAsteroid()
    {
        if (waitingBigAsteroids.Count > 0)
        {
            var asteroid = waitingBigAsteroids[0];
            waitingBigAsteroids.Remove(asteroid);
            runningBigAsteroids.Add(asteroid);
        }

    }
    private bool MoveAsteroid(Asteroid asteroid, string type)
    {
        asteroid.TopMargin = asteroid.TopMargin - 20 * (float)asteroid.GetVariable("SinX").Value;
        asteroid.LeftMargin = asteroid.LeftMargin + 20 * (float)asteroid.GetVariable("CosX").Value;
        
        if (asteroid.TopMargin < -200 || asteroid.TopMargin > 1280 || asteroid.LeftMargin < -200 || asteroid.LeftMargin > 2120)
        {
            switch (asteroid.BrowseName)
            {
                case "Asteroid_1":
                    asteroid.TopMargin = -200;
                    asteroid.LeftMargin = r.Next(0, 1920);
                    asteroid.GetVariable("SinX").Value = -1;
                    asteroid.GetVariable("CosX").Value = 0.3;
                    break;
                case "Asteroid_2":
                    asteroid.TopMargin = -200;
                    asteroid.LeftMargin = r.Next(0, 1920);
                    asteroid.GetVariable("SinX").Value = -1;
                    asteroid.GetVariable("CosX").Value = -0.7;
                    break;
                case "Asteroid_3":
                    asteroid.TopMargin = r.Next(0, 1080); ;
                    asteroid.LeftMargin = 2120;
                    asteroid.GetVariable("SinX").Value = -0.3;
                    asteroid.GetVariable("CosX").Value = -1;
                    break;
                case "Asteroid_4":
                    asteroid.TopMargin = r.Next(0, 1080);
                    asteroid.LeftMargin = 2120;
                    asteroid.GetVariable("SinX").Value = 0.7;
                    asteroid.GetVariable("CosX").Value = -1;
                    break;
                case "Asteroid_5":
                    asteroid.TopMargin = 1280;
                    asteroid.LeftMargin = r.Next(0, 1920);
                    asteroid.GetVariable("SinX").Value = 1;
                    asteroid.GetVariable("CosX").Value = 0.3;
                    break;
                case "Asteroid_6":
                    asteroid.TopMargin = 1280;
                    asteroid.LeftMargin = r.Next(0, 1920);
                    asteroid.GetVariable("SinX").Value = 1;
                    asteroid.GetVariable("CosX").Value = 0.7;
                    break;
                case "Asteroid_7":
                    asteroid.TopMargin = r.Next(0, 1080); ;
                    asteroid.LeftMargin = -200;
                    asteroid.GetVariable("SinX").Value = 0.3;
                    asteroid.GetVariable("CosX").Value = 1;
                    break;
                case "Asteroid_8":
                    asteroid.TopMargin = r.Next(0, 1080); ;
                    asteroid.LeftMargin = -200;
                    asteroid.GetVariable("SinX").Value = 0.7;
                    asteroid.GetVariable("CosX").Value = 1;
                    break;
                default:
                    asteroid.TopMargin = -200; 
                    asteroid.LeftMargin = -200;
                    break;
            }
            if (type == "Big")
            {
                runningBigAsteroids.Remove(asteroid);
                waitingBigAsteroids.Add(asteroid);
            }
            else if (type == "Medium")
            {
                runningMediumAsteroids.Remove(asteroid);
                waitingMediumAsteroids.Add(asteroid);
            }
            else if (type == "Mini")
            {
                runningMiniAsteroids.Remove(asteroid);
                waitingMiniAsteroids.Add(asteroid);
            }

            return true;
        }
        return false;
    }
    private void AsteroidMovements()
    {
        try
        {
            foreach (var asteroid in runningBigAsteroids)
            {
                if (MoveAsteroid(asteroid, "Big"))
                    break;
            }

            foreach (var asteroid in runningMediumAsteroids)
            {
                if (MoveAsteroid(asteroid, "Medium"))
                    break;
            }

            foreach (var asteroid in runningMiniAsteroids)
            {
                if (MoveAsteroid(asteroid, "Mini"))
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Info("AsteroidMovements", e.Message);
        }


    }
    #endregion

    #region bullets
    PeriodicTask shipMovements;
    PeriodicTask bulletMovements;
    IUAVariable turnCounterclockwise;
    IUAVariable turnClockwise;
    float sinX, cosX;
    Panel spaceShip;
    List<Bullet> availableBullets = new List<Bullet>();
    List<Bullet> shootedBullets = new List<Bullet>();

    private void InitializeBullets()
    {
        for (int i = 1; i <= 20; i++)
        {
            var bullet = InformationModel.Make<Bullet>("Bullet_" + i);
            bullet.TopMargin = 0;
            bullet.LeftMargin = 0;
            Owner.Add(bullet);
            availableBullets.Add(bullet);
        }
    }

    private void BulletMovements()
    {
        try
        {
            foreach (var bullet in shootedBullets)
            {
                bullet.TopMargin = bullet.TopMargin - 20 * (float)bullet.GetVariable("SinX").Value;
                bullet.LeftMargin = bullet.LeftMargin + 20 * (float)bullet.GetVariable("CosX").Value;
                if (bullet.TopMargin < -10 || bullet.TopMargin > 1090 || bullet.LeftMargin < -10 || bullet.LeftMargin > 1930)
                {
                    shootedBullets.Remove(bullet);
                    availableBullets.Add(bullet);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Log.Info("BulletMovements", e.Message);
        }

    }

    [ExportMethod]
    public void Shoot()
    {
        if (availableBullets.Count > 0)
        {
            var bullet = availableBullets[availableBullets.Count - 1];
            availableBullets.RemoveAt(availableBullets.Count - 1);
            shootedBullets.Add(bullet);
            bullet.TopMargin = spaceShip.TopMargin + 55;
            bullet.LeftMargin = spaceShip.LeftMargin + 65;
            bullet.GetVariable("SinX").Value = sinX;
            bullet.GetVariable("CosX").Value = cosX;
        }
        
    }
    #endregion

    #region SpaceShip
    private void ShipMovementLogic()
    {
        if (turnClockwise.Value)
            spaceShip.Rotation = spaceShip.Rotation + 20;
        if (turnCounterclockwise.Value)
            spaceShip.Rotation = spaceShip.Rotation - 20;

        sinX = (float)Math.Sin(-(Math.PI / 180) * spaceShip.Rotation);
        cosX = (float)Math.Cos(-(Math.PI / 180) * spaceShip.Rotation);

        float topMargin = spaceShip.TopMargin - 15 * sinX;
        float leftMargin = spaceShip.LeftMargin + 15 * cosX;

        if (topMargin > 0 && topMargin + spaceShip.Height < 1080 && leftMargin > 0 && leftMargin + spaceShip.Width < 1980)
        {
            spaceShip.TopMargin = topMargin;
            spaceShip.LeftMargin = leftMargin;
        }
    }



    #endregion
    public override void Start()
    {
        r = new Random();
        InitializeAsteroid();
        InitializeBullets();
        collisionLogic = new PeriodicTask(CollisionLogic, 500, LogicObject);
        collisionLogic.Start();
        bulletMovements = new PeriodicTask(BulletMovements, 100, LogicObject);
        bulletMovements.Start();

        //SpaceShip
        sinX = 0;
        cosX = 0;
        turnCounterclockwise = Owner.GetVariable("TurnCounterclockwise");
        turnClockwise = Owner.GetVariable("TurnClockwise");
        spaceShip = Owner.Get<Panel>("Ship");
        shipMovements = new PeriodicTask(ShipMovementLogic, 250, LogicObject);
        shipMovements.Start();

        //Asteroids
        
        bigAsteroid = new PeriodicTask(StartingBigAsteroid, 1500, LogicObject);
        bigAsteroid.Start();
        asteroidMovements = new PeriodicTask(AsteroidMovements, 500, LogicObject);
        asteroidMovements.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    private void InitializeAsteroid()
    {
        for (int i = 1; i <= 8; i++)
        {
            var asteroid = InformationModel.Make<Asteroid>("Asteroid_" + i);
            asteroid.Width = 200;
            asteroid.Height = 200;
            switch (i)
            {
                case 1:
                    asteroid.TopMargin = -200;
                    asteroid.LeftMargin = r.Next(0, 960);
                    asteroid.GetVariable("SinX").Value = -1;
                    asteroid.GetVariable("CosX").Value = 0.3;
                    break;
                case 2:
                    asteroid.TopMargin = -200;
                    asteroid.LeftMargin = r.Next(960, 1920);
                    asteroid.GetVariable("SinX").Value = -1;
                    asteroid.GetVariable("CosX").Value = -0.3;
                    break;
                case 3:
                    asteroid.TopMargin = r.Next(0, 540); ;
                    asteroid.LeftMargin = 2120;
                    asteroid.GetVariable("SinX").Value = -0.3;
                    asteroid.GetVariable("CosX").Value = -1;
                    break;
                case 4:
                    asteroid.TopMargin = r.Next(540, 1080);
                    asteroid.LeftMargin = 2120;
                    asteroid.GetVariable("SinX").Value = 0.3;
                    asteroid.GetVariable("CosX").Value = -1;
                    break;
                case 5:
                    asteroid.TopMargin = 1280;
                    asteroid.LeftMargin = r.Next(0, 960);
                    asteroid.GetVariable("SinX").Value = 1;
                    asteroid.GetVariable("CosX").Value = 0.3;
                    break;
                case 6:
                    asteroid.TopMargin = 1280;
                    asteroid.LeftMargin = r.Next(960, 1920);
                    asteroid.GetVariable("SinX").Value = 1;
                    asteroid.GetVariable("CosX").Value = -0.3;
                    break;
                case 7:
                    asteroid.TopMargin = r.Next(0, 540); ;
                    asteroid.LeftMargin = -200;
                    asteroid.GetVariable("SinX").Value = 0.3;
                    asteroid.GetVariable("CosX").Value = 1;
                    break;
                case 8:
                    asteroid.TopMargin = r.Next(540, 1080); ;
                    asteroid.LeftMargin = -200;
                    asteroid.GetVariable("SinX").Value = -0.3;
                    asteroid.GetVariable("CosX").Value = 1;
                    break;
                default:
                    break;
            }
            Owner.Add(asteroid);
            waitingBigAsteroids.Add(asteroid);
        }

        for (int i = 1; i <= 24; i++)
        {
            var asteroid = InformationModel.Make<Asteroid>("Asteroid_" + (i +8));
            asteroid.Width = 100;
            asteroid.Height = 100;
            asteroid.TopMargin = -200;
            asteroid.LeftMargin = -200;
            Owner.Add(asteroid);
            waitingMediumAsteroids.Add(asteroid);

        }

        for (int i = 1; i <= 72; i++)
        {
            var asteroid = InformationModel.Make<Asteroid>("Asteroid_" + (i+24));
            asteroid.Width = 50;
            asteroid.Height = 50;
            asteroid.TopMargin = -200;
            asteroid.LeftMargin = -200;
            Owner.Add(asteroid);
            waitingMiniAsteroids.Add(asteroid);

        }
    }

    
    private void CollisionLogic()
    {
        float topMargin = 0;
        float leftMargin = 0;
        try
        {
            foreach (var bullet in shootedBullets)
            {
                foreach (var asteroid in runningBigAsteroids)
                {

                    if (bullet.TopMargin > asteroid.TopMargin && bullet.TopMargin < asteroid.TopMargin + asteroid.Height && bullet.LeftMargin > asteroid.LeftMargin && bullet.LeftMargin < asteroid.LeftMargin + asteroid.Width)
                    {
                        topMargin = asteroid.TopMargin;
                        leftMargin = asteroid.LeftMargin;
                        switch (asteroid.BrowseName)
                        {
                            case "Asteroid_1":
                                asteroid.TopMargin = -200;
                                asteroid.LeftMargin = r.Next(0, 1920);
                                asteroid.GetVariable("SinX").Value = -1;
                                asteroid.GetVariable("CosX").Value = 0.3;
                                break;
                            case "Asteroid_2":
                                asteroid.TopMargin = -200;
                                asteroid.LeftMargin = r.Next(0, 1920);
                                asteroid.GetVariable("SinX").Value = -1;
                                asteroid.GetVariable("CosX").Value = -0.7;
                                break;
                            case "Asteroid_3":
                                asteroid.TopMargin = r.Next(0, 1080); ;
                                asteroid.LeftMargin = 2120;
                                asteroid.GetVariable("SinX").Value = -0.3;
                                asteroid.GetVariable("CosX").Value = -1;
                                break;
                            case "Asteroid_4":
                                asteroid.TopMargin = r.Next(0, 1080);
                                asteroid.LeftMargin = 2120;
                                asteroid.GetVariable("SinX").Value = 0.7;
                                asteroid.GetVariable("CosX").Value = -1;
                                break;
                            case "Asteroid_5":
                                asteroid.TopMargin = 1280;
                                asteroid.LeftMargin = r.Next(0, 1920);
                                asteroid.GetVariable("SinX").Value = 1;
                                asteroid.GetVariable("CosX").Value = 0.3;
                                break;
                            case "Asteroid_6":
                                asteroid.TopMargin = 1280;
                                asteroid.LeftMargin = r.Next(0, 1920);
                                asteroid.GetVariable("SinX").Value = 1;
                                asteroid.GetVariable("CosX").Value = 0.7;
                                break;
                            case "Asteroid_7":
                                asteroid.TopMargin = r.Next(0, 1080); ;
                                asteroid.LeftMargin = -200;
                                asteroid.GetVariable("SinX").Value = 0.3;
                                asteroid.GetVariable("CosX").Value = 1;
                                break;
                            case "Asteroid_8":
                                asteroid.TopMargin = r.Next(0, 1080); ;
                                asteroid.LeftMargin = -200;
                                asteroid.GetVariable("SinX").Value = 0.7;
                                asteroid.GetVariable("CosX").Value = 1;
                                break;
                            default:
                                break;
                        }
                        runningBigAsteroids.Remove(asteroid);
                        waitingBigAsteroids.Add(asteroid);
                        shootedBullets.Remove(bullet);
                        availableBullets.Add(bullet);
                        bullet.TopMargin = -20;
                        bullet.LeftMargin = -20;
                        for (int i = 1; i <= 3; i++)
                        {
                            var mediumAsteroid = waitingMediumAsteroids[0];
                            if (mediumAsteroid != null)
                            {
                                switch (i)
                                {
                                    case 1:
                                        mediumAsteroid.GetVariable("SinX").Value = 0.86;
                                        mediumAsteroid.GetVariable("CosX").Value = 0.5;
                                        break;
                                    case 2:
                                        mediumAsteroid.GetVariable("SinX").Value = 1;
                                        mediumAsteroid.GetVariable("CosX").Value = -0.5;
                                        break;
                                    case 3:
                                        mediumAsteroid.GetVariable("SinX").Value = -0.86;
                                        mediumAsteroid.GetVariable("CosX").Value = -0.5;
                                        break;
                                    default:
                                        break;
                                }
                                mediumAsteroid.TopMargin = topMargin + 50;
                                mediumAsteroid.LeftMargin = leftMargin + 50;
                                waitingMediumAsteroids.Remove(mediumAsteroid);
                                runningMediumAsteroids.Add(mediumAsteroid);
                            }
                        }
                        return;
                    }
                }
                foreach (var asteroid in runningMediumAsteroids)
                {

                    if (bullet.TopMargin > asteroid.TopMargin && bullet.TopMargin < asteroid.TopMargin + asteroid.Height && bullet.LeftMargin > asteroid.LeftMargin && bullet.LeftMargin < asteroid.LeftMargin + asteroid.Width)
                    {
                        topMargin = asteroid.TopMargin;
                        leftMargin = asteroid.LeftMargin;
                        asteroid.TopMargin = -200;
                        asteroid.LeftMargin = -200;

                        runningMediumAsteroids.Remove(asteroid);
                        waitingMediumAsteroids.Add(asteroid);
                        shootedBullets.Remove(bullet);
                        availableBullets.Add(bullet);
                        bullet.TopMargin = -20;
                        bullet.LeftMargin = -20;
                        for (int i = 1; i <= 3; i++)
                        {
                            var miniAsteroid = waitingMiniAsteroids[0];
                            if (miniAsteroid != null)
                            {
                                switch (i)
                                {
                                    case 1:
                                        miniAsteroid.GetVariable("SinX").Value = 0.86;
                                        miniAsteroid.GetVariable("CosX").Value = 0.5;
                                        break;
                                    case 2:
                                        miniAsteroid.GetVariable("SinX").Value = 1;
                                        miniAsteroid.GetVariable("CosX").Value = -0.5;
                                        break;
                                    case 3:
                                        miniAsteroid.GetVariable("SinX").Value = -0.86;
                                        miniAsteroid.GetVariable("CosX").Value = -0.5;
                                        break;
                                    default:
                                        break;
                                }
                                miniAsteroid.TopMargin = topMargin + 25;
                                miniAsteroid.LeftMargin = leftMargin + 25;
                                waitingMiniAsteroids.Remove(miniAsteroid);
                                runningMiniAsteroids.Add(miniAsteroid);
                            }
                        }
                        return;
                    }
                }
                foreach (var asteroid in runningMiniAsteroids)
                {

                    if (bullet.TopMargin > asteroid.TopMargin && bullet.TopMargin < asteroid.TopMargin + asteroid.Height && bullet.LeftMargin > asteroid.LeftMargin && bullet.LeftMargin < asteroid.LeftMargin + asteroid.Width)
                    {

                        asteroid.TopMargin = -200;
                        asteroid.LeftMargin = -200;

                        runningMiniAsteroids.Remove(asteroid);
                        waitingMiniAsteroids.Add(asteroid);
                        shootedBullets.Remove(bullet);
                        availableBullets.Add(bullet);
                        bullet.TopMargin = -20;
                        bullet.LeftMargin = -20;
                        return;
                    }
                }
            }

        }
        catch (Exception e)
        {
            Log.Info("CollisionLogic", e.Message);
        }
    }

}

