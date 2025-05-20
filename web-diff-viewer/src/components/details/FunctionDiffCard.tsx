import React from 'react';
import ExpandableCard from './ExpandableCard';
import type {FunctionDiff} from '../../diffs/types';
import PropertyDiffList from './PropertyDiffList';
import FlagsChangeView from './FlagsChangeView';

interface FunctionDiffCardProps {
    name: string;
    diff: FunctionDiff;
}

const FunctionDiffCard: React.FC<FunctionDiffCardProps> = ({name, diff}) => {
    return (
        <ExpandableCard
            title={`Function: ${name}`}
            status={diff.DiffType}
            contentStyle={{
                gap: 8,
            }}
        >
            <FlagsChangeView label='Flags' change={diff.FunctionFlags} />
            <PropertyDiffList properties={diff.InputProperties} />
            <PropertyDiffList properties={diff.OutputProperties} />
        </ExpandableCard>
    );
};

export default FunctionDiffCard;
