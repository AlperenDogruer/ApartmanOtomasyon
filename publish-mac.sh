#!/bin/bash
echo "============================================"
echo " Site Yonetim Otomasyon - macOS Build"
echo "============================================"
echo ""

# Apple Silicon (M1/M2/M3/M4)
echo "[1/3] Avalonia uygulamasi derleniyor (macOS ARM64 - Apple Silicon)..."
dotnet publish SiteYonetim.UI.Avalonia/SiteYonetim.UI.Avalonia.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishTrimmed=false \
    -o publish/macos-arm64

if [ $? -ne 0 ]; then
    echo ""
    echo "[HATA] ARM64 build basarisiz oldu!"
    exit 1
fi

# Intel Mac
echo ""
echo "[2/3] Avalonia uygulamasi derleniyor (macOS x64 - Intel)..."
dotnet publish SiteYonetim.UI.Avalonia/SiteYonetim.UI.Avalonia.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishTrimmed=false \
    -o publish/macos-x64

if [ $? -ne 0 ]; then
    echo ""
    echo "[HATA] x64 build basarisiz oldu!"
    exit 1
fi

# .app bundle olustur
echo ""
echo "[3/3] macOS .app bundle olusturuluyor..."

APP_NAME="SiteYonetim"
VERSION="1.0.0"

# ARM64 .app bundle
ARM64_APP="publish/macos-arm64/${APP_NAME}.app"
mkdir -p "${ARM64_APP}/Contents/MacOS"
mkdir -p "${ARM64_APP}/Contents/Resources"
cp publish/macos-arm64/SiteYonetim.UI.Avalonia "${ARM64_APP}/Contents/MacOS/${APP_NAME}"
chmod +x "${ARM64_APP}/Contents/MacOS/${APP_NAME}"
cp macos/Info.plist "${ARM64_APP}/Contents/Info.plist"

# x64 .app bundle
X64_APP="publish/macos-x64/${APP_NAME}.app"
mkdir -p "${X64_APP}/Contents/MacOS"
mkdir -p "${X64_APP}/Contents/Resources"
cp publish/macos-x64/SiteYonetim.UI.Avalonia "${X64_APP}/Contents/MacOS/${APP_NAME}"
chmod +x "${X64_APP}/Contents/MacOS/${APP_NAME}"
cp macos/Info.plist "${X64_APP}/Contents/Info.plist"

# ZIP olarak paketle
echo ""
echo "ZIP paketleri olusturuluyor..."
cd publish/macos-arm64
zip -r "../../installer_output/SiteYonetim_macOS_AppleSilicon_v${VERSION}.zip" "${APP_NAME}.app"
cd ../..

cd publish/macos-x64
zip -r "../../installer_output/SiteYonetim_macOS_Intel_v${VERSION}.zip" "${APP_NAME}.app"
cd ../..

echo ""
echo "Build tamamlandi!"
echo ""
echo " Apple Silicon: installer_output/SiteYonetim_macOS_AppleSilicon_v${VERSION}.zip"
echo " Intel Mac:     installer_output/SiteYonetim_macOS_Intel_v${VERSION}.zip"
echo ""
