import React, { useState } from 'react';
import type { DiffJson, AssetDiff } from './diffs/types';
import UploadArea from './components/UploadArea';
import Sidebar from './components/Sidebar';
import DetailsView from './components/details/DetailsView';

export default function App() {
    const [data, setData] = useState<DiffJson | AssetDiff | null>(null);
    const [selected, setSelected] = useState<AssetDiff | null>(null);
    const isSingleAsset = data && 'Name' in (data as AssetDiff);
    const isMultipleAssets = data && !isSingleAsset;

    return (
        <>
            {!data && <UploadArea onLoad={setData} />}
            {isSingleAsset && (
                <div style={{ display: 'flex', height: '100vh' }}>
                    <DetailsView asset={data as AssetDiff} />
                </div>
            )}
            {isMultipleAssets && (
                <div style={{ display: 'flex', height: '100vh' }}>
                    <Sidebar data={data as DiffJson} onSelect={setSelected} />
                    <DetailsView asset={selected} />
                </div>
            )}
        </>
    );
}
