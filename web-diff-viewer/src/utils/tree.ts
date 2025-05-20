
import type { AssetDiff, DiffType } from '../diffs/types';

export interface AssetTreeNode {
    name: string;
    fullName: string;
    status: DiffType;
    asset: AssetDiff | null;
    children: AssetTreeNode[];
}

type InternalTreeNode = {
    children: {
        [key: string]: InternalTreeNode;
    },
    asset?: AssetDiff;
};

export function buildAssetTree(assets: AssetDiff[]): AssetTreeNode[] {
    const root: InternalTreeNode = {} as InternalTreeNode;

    for (const asset of assets) {
        const parts = asset.Name.split('\\');
        let node = root;

        for (const part of parts) {
            if (!node.children) {
                node.children = {};
            }

            if (!node.children[part]) {
                node.children[part] = {} as InternalTreeNode;
            }

            node = node.children[part] as InternalTreeNode;
        }

        node.asset = asset;
    }

    function convert(node: InternalTreeNode, prefix = ''): AssetTreeNode[] {
        return Object.entries(node.children ?? {})
            .map(([key, child]) => {
                const fullName = prefix ? `${prefix}\\${key}` : key;

                return {
                    name: key,
                    fullName,
                    status: child.asset?.DiffType ?? 'Unchanged',
                    asset: child.asset ?? null,
                    children: convert(child, fullName),
                } as AssetTreeNode;
            });
    }

    return convert(root);
}
