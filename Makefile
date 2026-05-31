-include .env
export

UNITY      := /Applications/Unity/Hub/Editor/2022.3.20f1/Unity.app/Contents/MacOS/Unity
UNITY_ARGS := -batchmode -quit -projectPath . -buildTarget iOS -executeMethod BuildScript.BuildIOS -logFile unity-build.log

.PHONY: unity-build ios-beta

unity-build:
	$(UNITY) $(UNITY_ARGS)

ios-beta: unity-build
	bundle exec fastlane beta
