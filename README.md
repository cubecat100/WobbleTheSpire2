# WobbleTheSpire2

WobbleTheSpire2는 Slay the Spire 2용 피격 흔들림 모드입니다.

몬스터와 플레이어가 피격될 때 기본 hit/shake 반응 대신, 스프링처럼 튕기는 wobble 애니메이션을 재생합니다. 현재 구현은 아래쪽 pivot을 기준으로 회전하는 방식입니다.

## 기능

- 몬스터 피격 wobble
- 플레이어 피격 wobble
- 기본 피격 애니메이션 차단 옵션
- 사망 시 wobble 비활성화 옵션
- wobble 강도 조절 옵션
- hit 로그 출력 옵션
- 수평 이동 옵션
- squash and stretch 옵션
- stronger wobble 옵션
- longer wobble 옵션

## 기본 설정값

- Enable player wobble: `true`
- Block original hit animation: `true`
- Disable wobble on death: `true`
- Enable hit logs: `true`
- Enable horizontal wobble: `false`
- Enable squash and stretch: `false`
- Stronger wobble: `false`
- Longer wobble: `false`
- Overall wobble scale: `115`

설정 파일명:

- `wobblethespire2_settings.cfg`

기본 배포 경로:

- `D:\SteamLibrary\steamapps\common\Slay the Spire 2\mods\WobbleTheSpire2\wobblethespire2_settings.cfg`

## 프로젝트 구조

- `Scripts/`
  진입점, 전투 probe 등록, wobble 재생 로직
- `Patches/`
  Harmony 패치, 피격 감지, 기본 애니메이션 차단, 모딩 UI 연결
- `Settings/`
  설정 모델, 기본값, 로드, 저장
- `UI/`
  모드 설정 UI
- `mod_manifest.json`
  STS2 모드 매니페스트
- `WobbleTheSpire2.csproj`
  빌드 및 배포 설정

## 요구 사항

- .NET SDK
- Godot .NET SDK `4.5.1`
- `sts2.dll`

`sts2.dll`은 아래 둘 중 하나의 위치에서 찾습니다.

1. `.\sts2.dll`
2. `..\MapPathMod\.godot\mono\temp\bin\Debug\sts2.dll`

## 빌드

배포 없이 빌드만:

```powershell
dotnet build .\WobbleTheSpire2.csproj /p:SkipModDeployment=true
```

빌드 후 로컬 STS2 모드 폴더까지 배포:

```powershell
dotnet build .\WobbleTheSpire2.csproj
```

## 배포 방식

일반 빌드를 실행하면 `CopyToModsFolder` 타깃이 모드 폴더를 비운 뒤 아래 파일을 다시 복사합니다.

- `WobbleTheSpire2.dll`
- `mod_manifest.json`

기본 배포 폴더:

- `D:\SteamLibrary\steamapps\common\Slay the Spire 2\mods\WobbleTheSpire2`

게임 설치 경로가 다르면 `WobbleTheSpire2.csproj`의 `ModsFolder` 값을 수정해야 합니다.

## 멀티플레이 주의

현재 매니페스트에는 아래 값이 들어 있습니다.

- `"affects_gameplay": true`

이 값 때문에 멀티플레이에서 모드 동기화 검사가 발생할 수 있습니다.

## 디버그 로그

hit 로그가 켜져 있으면 다음 정보를 로그로 확인할 수 있습니다.

- 피격 감지 로그
- `AnimShake` 차단 여부
- `SetAnimationTrigger` 차단 여부

피격 감지와 기본 애니메이션 차단 동작을 검증할 때 사용합니다.

## 규칙

- 매니페스트 파일명은 다른 로컬 프로젝트와 동일하게 `mod_manifest.json`으로 유지합니다.
- 설정 파일은 `.json`이 아니라 `.cfg`를 사용합니다. STS2 로더가 별도 json 파일을 매니페스트로 오인하는 문제를 피하기 위한 목적입니다.
