# 加了
1. 玩家坦克(如果要做更多坦克，建議collider size: y = 0) prefab
   * 可以用WASD控制車身，砲塔會跟著車身轉但不會理游標??
     * 我游標動的時候砲塔的rotation y 會反應但大概只有1度的變化:D
     * 先找到到底炮管主視覺在哪
     * 也可能是因為我只加了armtank.gltf 好我打著打著突然悟了
2. 子彈 prefab
   * 還沒加反彈設定，現在碰到除bullet自己的layer以外的任何東西就會不見，or 5秒後生命結束
3. 
Assets/  
├── Scripts/  
│   ├── Player/  
│   │   ├── TankController.cs  
│   │   ├── TankShooting.cs  
│   │   └── PlayerHealth.cs  
│   ├── Enemy/  
│   │   └── EnemyTank.cs  
│   ├── Weapons/  
│   │   └── Bullet.cs  
│   └── GameManager.cs  
├── Scenes/  
│   └── SampleScene.unity  
├── gltf/ $\to$ 我們的main分支把這個位置放錯  
│   ├── Base.gltf  
│   ├── Barrel.gltf  
│   ├── Bullet.gltf  
│   └── ArmTank.gltf  
