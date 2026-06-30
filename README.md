# 🎮 PITA GAME

> Survival/Action RPG — inspirado em Vampire Survivors & Archero  
> Unity 2022.3 LTS · Android · iOS · PC

---

## 🚀 Setup Rápido

### Pré-requisitos
- Unity Hub + Unity **2022.3.60f1** (LTS)
- Android Build Support + NDK/JDK (para APK)
- iOS Build Support (para Xcode / IPA)
- Git LFS (assets binários)

### Abrir o Projeto
```bash
git clone https://github.com/<SEU_USER>/pita-game.git
```
1. Abra **Unity Hub → Add → Browse** e selecione a pasta `PitaGame/`
2. Aguarde a importação (primeira vez demora ~5 min)
3. Abra a cena **Assets/Scenes/Splash.unity**
4. Play ▶️

---

## 📁 Estrutura de Scripts

```
Assets/Scripts/
├── Core/           GameManager, GameBootstrapper
├── Player/         PlayerController, VirtualJoystick
├── Enemies/        EnemyBase, EnemyTypes, BossBase
├── Abilities/      AbilityBase, 5 abilities ativas + 10 passivas
├── Combat/         CombatEvents, CombatStats
├── Systems/        WaveManager, XPSystem, EnemyPool, ProjectilePool,
│                   DamageNumberPool, PickupItem, ChestSystem,
│                   AchievementSystem, DailyRewardSystem, SeasonPass,
│                   MinimapSystem, MapGenerator, RankingSystem
├── Progression/    PermanentUpgrades, CharacterData (30 personagens)
├── Shop/           ShopSystem
├── Save/           SaveSystem
├── UI/             UIManager, UIManagerExtensions, SplashScreen,
│                   MainMenuUI, SettingsUI, AbilitySelectionUI
├── Managers/       AudioManager, AdManager, FirebaseManager
├── Audio/          AudioManager
├── Utils/          SlowEffect, OrbitDamager
└── Data/           CurrencySystem, MissionSystem
```

---

## 🔧 Configuração da Cena (Inspector)

### GameBootstrapper (cena Splash)
Arraste todos os prefabs de sistemas nos slots do componente `GameBootstrapper`.

### UIManager
- Conecte todos os painéis de UI nos slots da inspetor
- Configure o `UIManagerExtensions` (parcial — mesma instância)

### AudioManager
- Adicione os `AudioClip`s nos slots `sfxClips[]` e `musicClips[]`

---

## 📱 Build Android (APK)

```
File → Build Settings → Android
Player Settings:
  - Company Name: PitaStudios
  - Product Name: Pita Game
  - Package Name: com.pitastudios.pitagame
  - Min API: 26 (Android 8)
  - Target API: 34 (Android 14)
  - Scripting Backend: IL2CPP
  - Architecture: ARM64
Build → Build And Run
```

---

## 🔥 Firebase Setup

1. Crie um projeto no [Firebase Console](https://console.firebase.google.com)
2. Baixe `google-services.json` → coloque em `Assets/`
3. Instale o [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup)
4. Descomente as chamadas reais em `FirebaseManager.cs`

---

## 💰 AdMob Setup

1. Crie app no [AdMob Console](https://admob.google.com)
2. Substitua os IDs placeholder em `AdManager.cs`
3. Instale [Google Mobile Ads Unity Plugin](https://github.com/googleads/googleads-mobile-unity)

---

## 🎯 Features Implementadas

| Sistema | Status |
|---------|--------|
| PlayerController (joystick + movimento 8 dir) | ✅ |
| 5 Habilidades ativas (Fireball, Lightning, Ice Aura, Orbiting Swords, Piercing Arrow) | ✅ |
| 10 Habilidades passivas (Vampirismo, Thorns, Shield, Speed, etc.) | ✅ |
| 9 tipos de inimigos + 3 bosses | ✅ |
| Sistema de ondas (WaveManager) | ✅ |
| Sistema de XP e Level Up | ✅ |
| Seleção de habilidades ao subir nível | ✅ |
| Progression permanente (25 upgrades) | ✅ |
| 30 Personagens desbloqueáveis | ✅ |
| Sistema de moedas e gemas | ✅ |
| Missões (MissionSystem) | ✅ |
| Conquistas (AchievementSystem) | ✅ |
| Recompensas diárias | ✅ |
| Passe de temporada (50 níveis) | ✅ |
| Loja (ShopSystem) | ✅ |
| Ranking local + Firebase | ✅ |
| Sistema de save (JSON cifrado) | ✅ |
| Object Pools (inimigos, projéteis, dano) | ✅ |
| Minimap | ✅ |
| Gerador de mapas procedural (20+ biomas) | ✅ |
| AdMob (Interstitial + Rewarded + Banner) | ✅ |
| Firebase (Analytics + Auth + Firestore + Remote Config) | ✅ |
| Splash screen épica com efeitos | ✅ |
| UI completa (Menu, Shop, Ranking, Settings, Season Pass) | ✅ |
| GitHub Actions → APK automático | ✅ |

---

## 🤖 CI/CD — APK Automático

Push na branch `main` → GitHub Actions gera o APK automaticamente.  
Download em **Actions → Artifacts → pita-game-apk**.

Segredos necessários no repositório:
- `UNITY_LICENSE` — conteúdo do arquivo `.ulf` da sua licença Unity
- `UNITY_EMAIL` — e-mail da conta Unity
- `UNITY_PASSWORD` — senha da conta Unity

---

## 📄 Licença

Propriedade de **Pita Studios**. Todos os direitos reservados.
