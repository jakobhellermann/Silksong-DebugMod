game := "C:/Users/Jakob/Documents/dev/contrib/RustyAssetBundleEXtractor/out"

examplemod:
    uvx unity-scene-repacker \
        --game-dir "{{game}}" \
        --objects Resources/bundle.objects.json \
        --output Resources/bundle.unity3d
