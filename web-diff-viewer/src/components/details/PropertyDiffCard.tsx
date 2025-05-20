import React from 'react';
import ExpandableCard from './ExpandableCard';
import type { PropertyDiff, FlagsChange } from '../../diffs/types';
import PropertyDiffList from './PropertyDiffList';
import ValueChangeView from './ValueChangeView';
import FlagsChangeView from './FlagsChangeView';

interface PropertyDiffCardProps {
    name: string;
    diff: PropertyDiff;
    prefix?: string;
}

const PropertyDiffCard: React.FC<PropertyDiffCardProps> = ({
    name,
    diff,
    prefix = 'Property'
}) => {
    return (
        <ExpandableCard
            title={`${prefix}: ${name}`}
            status={diff.DiffType}
            contentStyle={{
                gap: 8,
            }}
        >
            <FlagsChangeView label='Flags' change={diff.PropertyFlags} />
            <ValueChangeView label='Type' change={diff.Type} />
            <ValueChangeView label='StructClass' change={diff.StructClass} />
            <ValueChangeView label='PropertyClass' change={diff.PropertyClass} />
            <ValueChangeView label='ArrayDim' change={diff.ArrayDim} />
            <PropertyDiffList properties={diff.InnerProperties} />
        </ExpandableCard>
    );
};

export default PropertyDiffCard;
