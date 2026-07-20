# Enemy Path Debug Logs Plan

## Problem
Enemy'ler kendi base'lerinden spawn olup player base'e gidiyor, sonra tekrar kendi base'lerine geri dönüyor.

## Amaç
Sebebi tespit etmek için debug log'ları eklemek.

## Değişiklikler

### 1. Scripts/Combat/Unit.cs

**A) Waypoint takibine dönüş log'u (Update, ~line 292)**
```csharp
Debug.Log($"[PATH_DEBUG] {gameObject.name} | Side:{unitSide} | wpIdx:{currentWaypointIndex}/{currentPath.GetWaypoints().Count} | Pos:{transform.position} | TargetWpPos:{targetPos}");
```

**B1) Dark ilerleme log'u (~line 309)**
```csharp
Debug.Log($"[PATH_DEBUG] {gameObject.name} | DARK ADVANCED wpIdx:{currentWaypointIndex} | Dir:FORWARD");
```

**B2) Light ilerleme log'u (~line 300)**
```csharp
Debug.Log($"[PATH_DEBUG] {gameObject.name} | LIGHT ADVANCED wpIdx:{currentWaypointIndex} | Dir:BACKWARD");
```

**C) targetBase fallback log'u (~line 321)**
```csharp
Debug.Log($"[PATH_DEBUG] {gameObject.name} | FALLBACK to targetBase at {targetBase.transform.position} | currentMoveTarget:{currentMoveTarget} | wpIdx:{currentWaypointIndex}");
```

**D) OnReachPathEnd log'u (~line 630)**
```csharp
Debug.Log($"[PATH_DEBUG] {gameObject.name} | OnReachPathEnd CALLED | targetBase:{(targetBase!=null?targetBase.name:"NULL")} | Pos:{transform.position}");
```

### 2. Scripts/Combat/Soldier.cs

**E) Soldier rally point log'u (~line 70)**
```csharp
Debug.Log($"[PATH_DEBUG] {gameObject.name} | SOLDIER moving to rallyPoint:{rallyPoint} | isEnemy:{GetSide() != SideController.Instance.GetPlayerSide()}");
```

## Test
- Bir level'ı çalıştır
- Console'da `[PATH_DEBUG]` filtresiyle gözlem yap
- Enemy'nin base'e gidip döndüğü anda hangi log'ların bastığını rapor et
