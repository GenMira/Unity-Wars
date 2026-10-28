"""Run the actual Pure C# map flow without starting Unity or modifying generated csproj files."""
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
script_root = root / 'Assets/Scripts'
sources = [
    'Definition/BattleField.cs', 'Definition/Hexposition.cs', 'Definition/DataType.cs',
    'Definition/Data.cs', 'Definition/ITroop.cs', 'Definition/IFacility.cs',
    'Definition/TroopPlacement.cs', 'Domain/Model/MapDefinition.cs',
    'Domain/Model/MapCatalog.cs', 'Domain/Model/BoardState.cs',
    'Domain/Model/GameState.cs', 'Domain/Model/PlayerState.cs',
    'Domain/Model/TurnState.cs', 'Domain/Unit/Troop/Troop.cs',
    'Domain/Unit/Troop/Solider.cs', 'Domain/Unit/Facility/Facility.cs',
    'Domain/Unit/lib/NameSolver.cs', 'Domain/Unit/lib/TroopFactory.cs',
    'Domain/Service/GameFactory.cs', 'Domain/Service/TurnService.cs',
    'application/MapSelection.cs', 'application/SelectMapUseCase.cs',
    'application/CreateMatchUseCase.cs',
]
with tempfile.TemporaryDirectory(prefix='unity-wars-map-checks-') as folder:
    project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(project, 'PropertyGroup')
    for key, value in {
        'OutputType': 'Exe', 'TargetFramework': 'net10.0',
        'LangVersion': '9.0', 'EnableDefaultCompileItems': 'false',
        'ImplicitUsings': 'disable', 'Nullable': 'disable',
    }.items():
        ET.SubElement(props, key).text = value
    files = ET.SubElement(project, 'ItemGroup')
    for source in sources:
        ET.SubElement(files, 'Compile', Include=str(script_root / source))
    ET.SubElement(files, 'Compile', Include=str(root / 'tests/MapFlowChecks.cs'))
    for package in ['R3.1.3.1', 'Microsoft.Bcl.TimeProvider.8.0.0']:
        for dll in (root / 'Assets/Packages' / package / 'lib').glob('netstandard*/*.dll'):
            reference = ET.SubElement(files, 'Reference', Include=dll.stem)
            ET.SubElement(reference, 'HintPath').text = str(dll)
    path = Path(folder) / 'MapFlowChecks.csproj'
    ET.ElementTree(project).write(path, encoding='unicode')
    subprocess.run(['dotnet', 'run', '--project', str(path)], check=True, cwd=root)
