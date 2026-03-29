# Phase 04: 테스트 및 밸런싱

## 목표
- 에디터에서 Play 모드로 게임을 테스트한다.
- pile 크기, 발사 힘, 카메라 위치, 파괴 임계값 등을 밸런싱한다.
- 발견되는 버그를 수정한다.

## 선행 조건
- Phase 01 완료 (씬 준비)
- Phase 02 완료 (핵심 스크립트)
- Phase 03 완료 (게임 로직)
- 에디터가 실행 중이어야 한다

## 작업 (rose-cli + 코드 수정)

이 Phase는 **에디터 테스트 + 코드 밸런싱 반복** 작업이다.

### 4-1. Play 모드 진입
```
play.enter
```
- 게임이 시작되면 pile이 생성되고 cannonball이 shooter 위치에 스폰되는지 확인

### 4-2. 기본 동작 확인
- 마우스 드래그로 cannonball 발사가 되는지 확인
- cannonball이 pile에 충돌하면 블록이 파괴되는지 확인
- pig가 충돌 시 사망하는지 확인
- bomb 큐브가 폭발하는지 확인
- 스테이지 클리어 후 다음 스테이지로 진행되는지 확인

### 4-3. 밸런싱 항목

밸런싱이 필요한 경우 해당 스크립트의 상수를 수정한다.

| 항목 | 파일 | 상수 | 현재값 | 조정 방향 |
|------|------|------|--------|-----------|
| 발사 힘 감도 | AngryClawdGame.cs | SHOOT_FORCE_MULTIPLIER | 0.3f | 드래그가 짧아도 충분한 힘이면 올림, 너무 세면 내림 |
| 최대 발사 힘 | AngryClawdGame.cs | MAX_SHOOT_FORCE | 40f | cannonball이 pile을 관통하면 내림 |
| cannonball 질량 | AngryClawdGame.cs | CANNONBALL_MASS | 2.0f | 블록을 잘 밀어내지 못하면 올림 |
| 블록 파괴 속도(cannonball) | CannonballScript.cs | MIN_DESTROY_SPEED | 3.0f | 너무 쉽게 부서지면 올림 |
| 블록 연쇄 파괴 속도 | BlockScript.cs | BREAK_SPEED | 8.0f | 연쇄가 안 일어나면 내림 |
| pig 사망 속도 | PigScript.cs | KILL_SPEED | 2.0f | pig가 너무 안 죽으면 내림 |
| 폭탄 트리거 속도 | BombScript.cs | TRIGGER_SPEED | 2.0f | 폭탄이 안 터지면 내림 |
| 폭탄 범위 | BombScript.cs | EXPLOSION_RADIUS | 3.0f | 범위가 좁으면 올림 |
| 폭탄 확률 | PileScript.cs | BOMB_CHANCE | 0.05f | 폭탄이 너무 적으면 올림 |
| pile 간 간격 | AngryClawdGame.cs | PILE_SPACING | 8.0f | pile끼리 겹치면 넓힘 |
| 큐브 크기 | PileScript.cs | CUBE_SIZE | 0.8f | 시각적 크기 조정 |
| 블록 질량 | PileScript.cs | rb.mass = 0.5f | 0.5f | cannonball에 너무 잘 밀리면 올림 |
| cannonball 타임아웃 | AngryClawdGame.cs | CANNONBALL_TIMEOUT | 8.0f | 너무 오래 기다리면 내림 |

### 4-4. 카메라 미세 조정
```
transform.set_position <cam_id> <x,y,z>
transform.set_rotation <cam_id> <x,y,z>
```
- shooter 위치와 pile 영역이 모두 잘 보이는지 확인
- 카메라가 너무 멀거나 가까우면 위치 조정

### 4-5. Play 모드 종료
```
play.exit
```

### 4-6. 씬 저장 (카메라 등 변경 사항이 있을 경우)
```
scene.save
```

## 수정할 파일 (밸런싱 시)

각 파일의 상수 값만 변경하므로 구조적 변경은 없다.

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/AngryClawdGame.cs`
- **변경 내용**: SHOOT_FORCE_MULTIPLIER, MAX_SHOOT_FORCE, CANNONBALL_MASS 등 상수값 조정
- **이유**: 게임플레이 밸런싱

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/CannonballScript.cs`
- **변경 내용**: MIN_DESTROY_SPEED 상수값 조정
- **이유**: 파괴 난이도 밸런싱

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/BlockScript.cs`
- **변경 내용**: BREAK_SPEED 상수값 조정
- **이유**: 연쇄 파괴 빈도 밸런싱

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/PigScript.cs`
- **변경 내용**: KILL_SPEED 상수값 조정
- **이유**: pig 생존력 밸런싱

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/BombScript.cs`
- **변경 내용**: TRIGGER_SPEED, EXPLOSION_RADIUS 상수값 조정
- **이유**: 폭탄 효과 밸런싱

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/PileScript.cs`
- **변경 내용**: BOMB_CHANCE, CUBE_SIZE, rb.mass 값 조정
- **이유**: pile 구성 밸런싱

## 빌드 명령
```bash
cd /home/alienspy/git/MyGame && dotnet build LiveCode/LiveCode.csproj
```

## 검증 기준
- [ ] `dotnet build` 성공
- [ ] Play 모드에서 기본 게임 플로우가 동작함:
  - cannonball 발사 가능
  - 블록 파괴 동작
  - pig 사망 동작
  - 폭탄 폭발 동작
  - 스테이지 클리어 -> 다음 스테이지 진행
- [ ] 밸런싱 후 게임이 플레이 가능한 수준
- [ ] 카메라에서 전체 필드가 잘 보임

## 참고
- IronRose는 LiveCode 핫 리로드를 지원하므로, Play 모드를 나가지 않고도 코드 수정 후 저장하면 변경 사항이 반영될 수 있다.
- 하지만 상수(const) 변경은 핫 리로드로 반영되지 않을 수 있으므로, Play 모드를 종료하고 다시 진입하는 것이 확실하다.
- 이 Phase는 반복적 작업이므로, 한 번에 완료하기보다 여러 차례 조정-테스트 사이클을 거칠 수 있다.
- 심각한 버그가 발견되면 해당 스크립트를 수정하고 빌드를 확인한 후 다시 테스트한다.
