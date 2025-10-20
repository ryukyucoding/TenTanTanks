# 加了
## 玩家坦克
   * Collider size = (1, 1, 1)
   * Collider center = (0, 0.5, 0)
## 敵人坦克
   * AI 是隨便亂寫的
   * Collider size = (1, 1, 1)
   * Collider center = (0, 0.5, 0)
## 子彈
   * 還沒加反彈設定，但可以打人了
## Files 
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

## 砲管啦
把坦克包成：
PlayerTank/  
├── ArmTank.fbx (裡面的Barrel.001取消勾選)  
├── Turret  
│   ├── Barrel.fbx  
│   └── FirePoint (0, 0.25, 0.49)  

讓車身跟砲管獨立，滑鼠才追蹤的到
反正一切都可以複製

## 10/20 + Explosion
音效+爆炸特效，敵人的確定可以用但玩家不確定，要先跟AI merge才能知道有沒有bug
