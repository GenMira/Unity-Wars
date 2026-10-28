"""Run real Unity Editor/native Tilemap checks in an isolated temporary project."""
from pathlib import Path
import argparse
import json
import shutil
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--unity', default='/Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app/Contents/MacOS/Unity')
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
with tempfile.TemporaryDirectory(prefix='unity-wars-tilemap-checks-') as directory:
    project = Path(directory)
    def copy(relative):
        source = root / relative
        target = project / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)
        meta = Path(str(source) + '.meta')
        if meta.exists(): shutil.copy2(meta, str(target) + '.meta')
    for relative in [
        'ProjectSettings/ProjectVersion.txt',
        'Assets/Scripts/Definition/DataType.cs', 'Assets/Scripts/Definition/Hexposition.cs',
        'Assets/Scripts/Definition/BattleField.cs', 'Assets/Scripts/Domain/Model/MapDefinition.cs',
        'Assets/Scripts/CompositionRoot/MapDefinitionAsset.cs',
        'Assets/Maps/DefaultMapTileCatalog.asset',
        'Assets/Scenes/Battle.unity',
    ]: copy(relative)
    for folder in ['Assets/Scripts/Authoring', 'Assets/Editor']:
        for path in (root / folder).glob('*.cs'): copy(str(path.relative_to(root)))
    for path in (root / 'Assets/Graphics').glob('*'):
        if path.suffix in ['.asset', '.png']: copy(str(path.relative_to(root)))
    tests = project / 'Assets/Editor/TilemapAuthoringChecks.cs'
    shutil.copy2(root / 'tests/TilemapAuthoringChecks.cs', tests)
    packages = project / 'Packages'
    packages.mkdir(exist_ok=True)
    (packages / 'manifest.json').write_text(json.dumps({'dependencies': {
        'com.unity.modules.tilemap': '1.0.0',
        'com.unity.modules.imgui': '1.0.0',
        'com.unity.modules.imageconversion': '1.0.0',
    }}))
    log = project / 'checks.log'
    print('Unity checks project:', project, flush=True)
    result = subprocess.run([
        args.unity, '-batchmode', '-nographics', '-projectPath', str(project),
        '-executeMethod', 'TilemapAuthoringChecks.Run', '-logFile', str(log),
    ], timeout=300)
    lines = log.read_text(errors='replace').splitlines() if log.exists() else []
    for line in lines:
        if 'MAPCHECK' in line or 'error CS' in line or 'Scripts have compiler errors' in line:
            print(line)
    if result.returncode != 0 or not any('MAPCHECK RESULT' in line for line in lines):
        failure_log = Path(tempfile.gettempdir()) / 'unity-wars-tilemap-checks-failure.log'
        if log.exists(): shutil.copy2(log, failure_log)
        print('Failure log:', failure_log)
        raise SystemExit(result.returncode or 1)
