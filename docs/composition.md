#Domain
GameData
BoardData
TroopData

ITroop
Solider
Tank
…etc.

BattleService
OccupationService

BattleService - - >GameData,ITrop
OccupationService - - >GameData,ITrop
GameData o - - BoardData,TroopData
ITroop - - |>Solider,Tank,etc.

#application
StartGameUseCase
EndGameUseCase

OccupationUseCase
MoveTroopUseCase
AttackTroopUseCase

StartGameUseCase . . >GameData
EndGameUseCase . . >GameData
OccupationUseCase - - >OccupationService
MoveTroopUseCase - - >BattleService
AttackTroopUseCase - - >BattleService

#presentation
BoardView
BoardPresentor
TroopView
TroopPresentor
UIView
UIPresenter

BoardPresenter - - >BoardView
TroopPresenter - - >TroopView
UIPresenter - - >UIView
UIPresenter - - >OccupationUseCase,MoveTroopUseCase,AttackTroopUseCase
BoardPresenter . . >GameData
TroopPresenter . . >GameData

#CompositionRoot
 GameLifeTimeScope

 GameLifeTimeScope - - >Presentation,Application,Domain
